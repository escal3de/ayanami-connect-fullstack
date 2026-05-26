namespace AyanamiConnect.Application.Contracts.PanelClients;

public record PanelClientResponse(
    Guid Id,
    string Email,
    string Uuid,
    string SubId,
    long ExpiryTime,
    long TotalGB,
    int LimitIp,
    string Flow,
    bool Enable,
    int Reset);