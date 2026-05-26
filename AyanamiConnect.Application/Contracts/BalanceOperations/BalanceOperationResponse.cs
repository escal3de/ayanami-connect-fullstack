namespace AyanamiConnect.Application.Contracts.BalanceOperations;

public record BalanceOperationResponse(
    Guid Id,
    string Kind,
    string Title,
    decimal Amount,
    string Note,
    DateTime CreatedAt);
