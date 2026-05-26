using AyanamiConnect.Application.Abstractions.EternalServices;
using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Application.Contracts.ThreeXUi;
using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Application.Mapping;
using AyanamiConnect.Domain;
using AyanamiConnect.Domain.Enums;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Handlers.Subscriptions;

public class ExtendSubscriptionHandler(
    IUsersRepository repository,
    IThreeXUiClientsService clientsService)
{
    private readonly IUsersRepository _repository = repository;
    private readonly IThreeXUiClientsService _clientsService = clientsService;

    public async Task<Result<UserResponse>> HandleAsync(
        long telegramId,
        SubscriptionPlans plan,
        CancellationToken cancellationToken)
    {
        var userResult = await GetLatestUserAsync(telegramId, cancellationToken);
        if (userResult.IsFailure)
            return Result.Failure<UserResponse>(userResult.Error);

        var user = userResult.Value;
        var subscription = GetLatestSubscription(user);
        if (subscription is null)
            return Result.Failure<UserResponse>("User has no subscription to extend.");

        var panelClients = GetSubscriptionPanelClients(user, subscription);
        if (panelClients.Count == 0)
            return Result.Failure<UserResponse>("No panel clients were found for the subscription.");

        var price = GetPrice(plan);
        if (!user.TryWithdrawFromBalance(price, out var balanceError))
            return Result.Failure<UserResponse>(balanceError);

        var rollback = CreateRollbackState(user, subscription, panelClients);
        var template = panelClients.First();
        var previousRequest = BuildRequest(template);
        var syncRequest = previousRequest;

        try
        {
            var duration = GetDuration(plan);
            var baseDate = GetExtensionBaseDate(subscription, panelClients);
            subscription.ExtendBy(duration, baseDate);
            subscription.ChangePlan(plan, price);

            var operationResult = BalanceOperation.Create(
                BalanceOperationKind.Subscription,
                "Покупка подписки",
                -price,
                $"Оплата подписки {GetPlanLabel(plan)}.");

            if (operationResult.IsFailure)
                return await RollbackAndFailAsync(user, subscription, panelClients, rollback, previousRequest, syncRequest, operationResult.Error, restorePanel: true, cancellationToken);

            user.AddBalanceOperation(operationResult.Value);

            var newExpiryTime = new DateTimeOffset(subscription.EndedAt).ToUnixTimeMilliseconds();
            foreach (var panelClient in panelClients)
            {
                panelClient.SetExpiryTime(newExpiryTime);
                panelClient.SetEnable(true);
            }

            syncRequest = previousRequest with
            {
                ExpiryTime = newExpiryTime,
                Enable = true
            };

            var deleteResult = await DeletePanelClientAsync(template, cancellationToken);
            if (deleteResult.IsFailure)
                return await RollbackAndFailAsync(user, subscription, panelClients, rollback, previousRequest, syncRequest, deleteResult.Error, restorePanel: false, cancellationToken);

            var createResult = await _clientsService.CreateClientAsync(syncRequest, cancellationToken);
            if (createResult.IsFailure)
                return await RollbackAndFailAsync(user, subscription, panelClients, rollback, previousRequest, syncRequest, createResult.Error, restorePanel: true, cancellationToken);

            try
            {
                await _repository.UpdateAsync(user, cancellationToken);
            }
            catch (Exception ex)
            {
                return await RollbackAndFailAsync(user, subscription, panelClients, rollback, previousRequest, syncRequest, ex.Message, restorePanel: true, cancellationToken);
            }

            return Result.Success(user.ToResponse());
        }
        catch (Exception ex)
        {
            return await RollbackAndFailAsync(user, subscription, panelClients, rollback, previousRequest, syncRequest, ex.Message, restorePanel: true, cancellationToken);
        }
    }

    private async Task<Result<User>> GetLatestUserAsync(long telegramId, CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllByTelegramIdAsync(telegramId, cancellationToken);

        if (users.Count == 0)
            return Result.Failure<User>($"User with telegram id {telegramId} not found.");

        return Result.Success(users.OrderByDescending(x => x.CreatedAt).First());
    }

    private static Subscription? GetLatestSubscription(User user)
        => user.Subscriptions.OrderByDescending(x => x.EndedAt).FirstOrDefault();

    private static List<PanelClient> GetSubscriptionPanelClients(User user, Subscription subscription)
        => user.PanelClients
            .Where(x => Normalize(x.SubId) == Normalize(subscription.Id.ToString("N")))
            .ToList();

    private static SubscriptionRollbackState CreateRollbackState(User user, Subscription subscription, IReadOnlyList<PanelClient> panelClients)
        => new(
            user.Balance,
            user.BalanceOperations
                .Select(operation => BalanceOperation.Load(
                    operation.Id,
                    operation.Kind,
                    operation.Title,
                    operation.Amount,
                    operation.Note,
                    operation.CreatedAt))
                .ToList(),
            Subscription.Load(
                subscription.Email,
                subscription.Id,
                subscription.Name,
                subscription.StartedAt,
                subscription.EndedAt,
                subscription.Price,
                subscription.Status,
                subscription.Plans),
            panelClients.ToDictionary(
                panelClient => panelClient.Id,
                panelClient => PanelClient.Load(
                    panelClient.Id,
                    panelClient.Email,
                    panelClient.Uuid,
                    panelClient.SubId,
                    panelClient.ExpiryTime,
                    panelClient.TotalGB,
                    panelClient.LimitIp,
                    panelClient.Flow,
                    panelClient.Enable,
                    panelClient.Reset)));

    private async Task<Result<UserResponse>> RollbackAndFailAsync(
        User user,
        Subscription subscription,
        IReadOnlyList<PanelClient> panelClients,
        SubscriptionRollbackState rollback,
        CreatePanelClientRequest previousRequest,
        CreatePanelClientRequest currentRequest,
        string error,
        bool restorePanel,
        CancellationToken cancellationToken)
    {
        RestoreState(user, subscription, panelClients, rollback);

        if (restorePanel)
        {
            var rollbackResult = await RollbackPanelAsync(previousRequest, currentRequest, cancellationToken);
            if (rollbackResult.IsFailure)
                return Result.Failure<UserResponse>($"{error} Rollback failed: {rollbackResult.Error}");
        }

        return Result.Failure<UserResponse>(error);
    }

    private static void RestoreState(
        User user,
        Subscription subscription,
        IReadOnlyList<PanelClient> panelClients,
        SubscriptionRollbackState rollback)
    {
        user.RestoreBalance(rollback.Balance);
        user.BalanceOperations.Clear();
        user.BalanceOperations.AddRange(rollback.BalanceOperations);
        subscription.RestoreFrom(rollback.Subscription);

        foreach (var panelClient in panelClients)
        {
            if (rollback.PanelClients.TryGetValue(panelClient.Id, out var panelClientSnapshot))
                panelClient.RestoreFrom(panelClientSnapshot);
        }
    }

    private async Task<Result> RollbackPanelAsync(
        CreatePanelClientRequest previousRequest,
        CreatePanelClientRequest currentRequest,
        CancellationToken cancellationToken)
    {
        await TryDeleteClientAsync(currentRequest, cancellationToken);

        var restoreResult = await _clientsService.CreateClientAsync(previousRequest, cancellationToken);
        return restoreResult.IsFailure
            ? Result.Failure(restoreResult.Error)
            : Result.Success();
    }

    private async Task<Result> DeletePanelClientAsync(PanelClient template, CancellationToken cancellationToken)
    {
        var deleteResult = await _clientsService.DeleteClientBySubIdAsync(template.SubId, cancellationToken);

        if (deleteResult.IsFailure)
            deleteResult = await _clientsService.DeleteClientByUuidAsync(template.Uuid, cancellationToken);

        if (deleteResult.IsFailure)
            deleteResult = await _clientsService.DeleteClientByEmailAsync(template.Email, cancellationToken);

        return deleteResult.IsFailure
            ? Result.Failure(deleteResult.Error)
            : Result.Success();
    }

    private async Task<Result> TryDeleteClientAsync(CreatePanelClientRequest request, CancellationToken cancellationToken)
    {
        var deleteResult = await _clientsService.DeleteClientBySubIdAsync(request.SubId, cancellationToken);

        if (deleteResult.IsFailure)
            deleteResult = await _clientsService.DeleteClientByUuidAsync(request.Id, cancellationToken);

        if (deleteResult.IsFailure)
            deleteResult = await _clientsService.DeleteClientByEmailAsync(request.Email, cancellationToken);

        return deleteResult.IsFailure
            ? Result.Failure(deleteResult.Error)
            : Result.Success();
    }

    private static TimeSpan GetDuration(SubscriptionPlans plan)
        => plan switch
        {
            SubscriptionPlans.Trial => TimeSpan.FromDays(1),
            SubscriptionPlans.Monthly => TimeSpan.FromDays(30),
            SubscriptionPlans.Quarterly => TimeSpan.FromDays(90),
            SubscriptionPlans.Yearly => TimeSpan.FromDays(365),
            _ => throw new ArgumentOutOfRangeException(nameof(plan), plan, "Unsupported subscription plan.")
        };

    private static DateTime GetExtensionBaseDate(Subscription subscription, IReadOnlyList<PanelClient> panelClients)
    {
        var candidates = new List<DateTime>
        {
            subscription.EndedAt,
            DateTime.UtcNow
        };

        candidates.AddRange(panelClients
            .Select(panelClient => DateTimeOffset.FromUnixTimeMilliseconds(panelClient.ExpiryTime).UtcDateTime)
            .Where(value => value > DateTime.MinValue));

        return candidates.Max();
    }

    private static decimal GetPrice(SubscriptionPlans plan)
        => plan switch
        {
            SubscriptionPlans.Trial => 0m,
            SubscriptionPlans.Monthly => 179m,
            SubscriptionPlans.Quarterly => 999m,
            SubscriptionPlans.Yearly => 1999m,
            _ => 0m
        };

    private static string GetPlanLabel(SubscriptionPlans plan)
        => plan switch
        {
            SubscriptionPlans.Trial => "trial",
            SubscriptionPlans.Monthly => "на месяц",
            SubscriptionPlans.Quarterly => "на 3 месяца",
            SubscriptionPlans.Yearly => "на год",
            _ => plan.ToString()
        };

    private static string Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Replace("-", string.Empty).Trim().ToUpperInvariant();

    private static CreatePanelClientRequest BuildRequest(PanelClient panelClient)
        => new()
        {
            Id = panelClient.Uuid,
            Email = panelClient.Email,
            Flow = panelClient.Flow,
            LimitIp = panelClient.LimitIp,
            TotalGB = panelClient.TotalGB,
            ExpiryTime = panelClient.ExpiryTime,
            Enable = panelClient.Enable,
            TgId = string.Empty,
            SubId = panelClient.SubId,
            Comment = string.Empty,
            Reset = panelClient.Reset
        };

    private sealed record SubscriptionRollbackState(
        decimal Balance,
        IReadOnlyList<BalanceOperation> BalanceOperations,
        Subscription Subscription,
        IReadOnlyDictionary<Guid, PanelClient> PanelClients);
}
