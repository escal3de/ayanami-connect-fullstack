namespace AyanamiConnect.Persistence.Entities;

public class InboundEntity
{
    public Guid Id { get; set; }
    public int PanelInboundId { get; set; }
    public string Remark { get; set; } = string.Empty;
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MaxClientsLimit { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SubscriptionEntity> Subscriptions { get; set; } = new();
}