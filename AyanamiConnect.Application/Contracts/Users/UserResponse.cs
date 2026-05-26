using AyanamiConnect.Application.Contracts.PanelClients;
using AyanamiConnect.Application.Contracts.Subscriptions;

namespace AyanamiConnect.Application.Contracts.Users;

public record UserResponse(
    Guid Id,
    long TelegramId,
    string? UserName,
    string FirstName,
    string? LastName,
    string LanguageCode,
    decimal Balance,
    string Role,
    DateTime CreatedAt,
    DateTime LastActiveAt,
    List<PanelClientResponse> PanelClients,
    List<SubscriptionResponse> Subscriptions);