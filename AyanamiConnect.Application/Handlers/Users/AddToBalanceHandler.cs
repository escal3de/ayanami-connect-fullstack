using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Application.Mapping;
using AyanamiConnect.Domain;
using AyanamiConnect.Domain.Enums;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Handlers.Users;

public class AddToBalanceHandler(IUsersRepository repository)
{
    private readonly IUsersRepository _repository = repository;

    public async Task<Result<UserResponse>> HandleAsync(long telegramId, decimal amount, CancellationToken cancellationToken)
    {
        if (amount <= 0)
            return Result.Failure<UserResponse>("Can not add 0 to balance");

        var user = await _repository.GetByTelegramIdAsync(telegramId, cancellationToken);

        if (user is null)
            return Result.Failure<UserResponse>("User not found");

        user.AddToBalance(amount);

        var operationResult = BalanceOperation.Create(
            BalanceOperationKind.Deposit,
            "Пополнение баланса",
            amount,
            "Пополнение через кабинет.");

        if (operationResult.IsFailure)
            return Result.Failure<UserResponse>(operationResult.Error);

        user.AddBalanceOperation(operationResult.Value);

        await _repository.UpdateAsync(user, cancellationToken);

        return Result.Success(user.ToResponse());
    }
}
