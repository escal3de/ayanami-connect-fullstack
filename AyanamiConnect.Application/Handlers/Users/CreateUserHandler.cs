using AyanamiConnect.Application.Abstractions.EternalServices;
using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Application.Contracts.ThreeXUi;
using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;

namespace AyanamiConnect.Application.Handlers.Users;

public class CreateUserHandler(
    IUsersRepository repository,
    IThreeXUiClientsService clientService,
    IValidator<CreateUserRequest> validator)
{
    private readonly IUsersRepository _repository = repository;
    private readonly IThreeXUiClientsService _clientService = clientService;
    private readonly IValidator<CreateUserRequest> _validator = validator;

    public async Task<Result> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result.Failure(string.Join(string.Empty, validationResult.Errors.Select(x => x.ErrorMessage)));

        var existingUsers = await _repository.GetAllByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (existingUsers.Count != 0)
            return Result.Failure($"User with telegram id {request.TelegramId} already exists.");

        var user = User.Create(request.TelegramId, request.UserName, request.FirstName, request.LastName);
        if (user.IsFailure)
            return Result.Failure(user.Error);

        var subId = Guid.NewGuid();
        var baseEmail = $"tg-{request.TelegramId}";

        var panelClientRequest = new CreatePanelClientRequest
        {
            Email = baseEmail,
            Enable = true,
            Id = Guid.NewGuid().ToString(),
            SubId = subId.ToString("N"),
            ExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddDays(1)).ToUnixTimeMilliseconds(),
            TotalGB = 0,
            TgId = request.TelegramId.ToString(),
            LimitIp = 3,
            Flow = string.Empty,
            Reset = 0,
            Comment = "Trial"
        };

        var panelClient = PanelClient.Create(
            Guid.NewGuid(),
            panelClientRequest.Email,
            panelClientRequest.Id,
            panelClientRequest.SubId,
            panelClientRequest.ExpiryTime,
            panelClientRequest.TotalGB,
            panelClientRequest.LimitIp,
            panelClientRequest.Flow,
            panelClientRequest.Enable,
            panelClientRequest.Reset);

        if (panelClient.IsFailure)
            return Result.Failure(panelClient.Error);

        var subscription = Subscription.Create(
            subId,
            panelClientRequest.Email,
            "Trial",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1));

        if (subscription.IsFailure)
            return Result.Failure(subscription.Error);

        user.Value.PanelClients.Add(panelClient.Value);
        user.Value.Subscriptions.Add(subscription.Value);

        Console.WriteLine("Before CreateClientAsync");
        var create = await _clientService.CreateClientAsync(panelClientRequest, cancellationToken);
        Console.WriteLine($"CreateClientAsync result: {create.IsSuccess}");

        if (create.IsFailure)
            return Result.Failure(create.Error);

        await _repository.AddAsync(user.Value, cancellationToken);

        return Result.Success();
    }
}
