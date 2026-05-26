namespace AyanamiConnect.Application.Contracts.ThreeXUi;

public record PanelClientResponse
{
    public int Id { get; init; }
    public int InboundId { get; init; }
    public bool Enable { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Uuid { get; init; } = string.Empty;
    public string SubId { get; init; } = string.Empty;
    public long Up { get; init; }
    public long Down { get; init; }
    public long AllTime { get; init; }
    public long ExpiryTime { get; init; }
    public long Total { get; init; }
    public int Reset { get; init; }
    public long LastOnline { get; init; }
}