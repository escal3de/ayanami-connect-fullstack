using AyanamiConnect.Domain.Enums;

namespace AyanamiConnect.Persistence.Entities;

public class BalanceOperationEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public BalanceOperationKind Kind { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
