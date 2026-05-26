using AyanamiConnect.Domain.Enums;

namespace AyanamiConnect.Persistence.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }
    public string? UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public List<BalanceOperationEntity> BalanceOperations { get; set; } = new();
    public List<PanelClientEntity> PanelClients { get; set; } = new();
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
}
