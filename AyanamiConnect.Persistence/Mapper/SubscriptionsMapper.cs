using AyanamiConnect.Domain;
using AyanamiConnect.Persistence.Entities;

namespace AyanamiConnect.Persistence.Mapper;

public static class SubscriptionsMapper
{
    public static SubscriptionEntity ToEntity(this Subscription subscription, Guid userId, Guid? inboundId = null)
        => new SubscriptionEntity
        {
            Email = subscription.Email,
            Id = subscription.Id,
            Name = subscription.Name,
            StartedAt = subscription.StartedAt,
            EndedAt = subscription.EndedAt,
            Price = subscription.Price,
            Status = subscription.Status,
            Plans = subscription.Plans,
            
            UserId = userId,
            InboundId = inboundId
        };

    public static Subscription ToDomain(this SubscriptionEntity subscription)
        => Subscription.Load(
            subscription.Email,
            subscription.Id,
            subscription.Name,
            subscription.StartedAt,
            subscription.EndedAt,
            subscription.Price,
            subscription.Status,
            subscription.Plans);
}
