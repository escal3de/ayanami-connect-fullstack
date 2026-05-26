using AyanamiConnect.Domain.Enums;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Domain;

public class BalanceOperation
{
    public Guid Id { get; private set; }
    public BalanceOperationKind Kind { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Note { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private BalanceOperation(
        Guid id,
        BalanceOperationKind kind,
        string title,
        decimal amount,
        string note,
        DateTime createdAt)
    {
        Id = id;
        Kind = kind;
        Title = title;
        Amount = amount;
        Note = note;
        CreatedAt = createdAt;
    }

    public static Result<BalanceOperation> Create(
        BalanceOperationKind kind,
        string title,
        decimal amount,
        string note,
        DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<BalanceOperation>("Balance operation title cannot be empty.");

        return Result.Success(new BalanceOperation(
            Guid.NewGuid(),
            kind,
            title,
            amount,
            note,
            createdAt ?? DateTime.UtcNow));
    }

    public static BalanceOperation Load(
        Guid id,
        BalanceOperationKind kind,
        string title,
        decimal amount,
        string note,
        DateTime createdAt)
        => new(id, kind, title, amount, note, createdAt);
}
