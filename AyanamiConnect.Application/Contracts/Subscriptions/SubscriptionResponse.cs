namespace AyanamiConnect.Application.Contracts.Subscriptions;

public record SubscriptionResponse(
    string Email,
    Guid Id,
    string Name,
    DateTime StartedAt,
    DateTime EndedAt,
    decimal Price,
    string Status,
    string Plans);