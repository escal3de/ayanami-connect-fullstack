using AyanamiConnect.Application.Contracts.Subscriptions;
using AyanamiConnect.Domain;

namespace AyanamiConnect.Application.Mapping;

public static class SubscriptionsMapper
{
    public static SubscriptionResponse ToResponse(this Subscription subscription)
        => new SubscriptionResponse(
            subscription.Email,
            subscription.Id,
            subscription.Name,
            subscription.StartedAt,
            subscription.EndedAt,
            subscription.Price,
            subscription.Status.ToString(),
            subscription.Plans.ToString());
}