using AyanamiConnect.Domain.Enums;

namespace AyanamiConnect.Persistence.Entities;

public class SubscriptionEntity
{
    public string Email { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public decimal Price { get; set; }
    public SubscriptionStatus Status { get; set; }
    public SubscriptionPlans Plans { get; set; }
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public Guid? InboundId { get; set; }
    public InboundEntity? Inbound { get; set; }
}