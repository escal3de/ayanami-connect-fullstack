using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Application.Contracts.BalanceOperations;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Handlers.BalanceOperations;

public class GetBalanceOperationsHandler(IUsersRepository repository)
{
    private readonly IUsersRepository _repository = repository;

    public async Task<Result<IReadOnlyList<BalanceOperationResponse>>> HandleAsync(
        long telegramId,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByTelegramIdAsync(telegramId, cancellationToken);

        if (user is null)
            return Result.Failure<IReadOnlyList<BalanceOperationResponse>>("User not found");

        var operations = user.BalanceOperations
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BalanceOperationResponse(
                x.Id,
                x.Kind.ToString().ToLowerInvariant(),
                x.Title,
                x.Amount,
                x.Note,
                x.CreatedAt))
            .ToList();

        return Result.Success<IReadOnlyList<BalanceOperationResponse>>(operations);
    }
}
