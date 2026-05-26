namespace AyanamiConnect.Infrastructure.ThreeXUi.Contracts;

public sealed class ClientStatResponse
{
    public int Id { get; set; }
    public int InboundId { get; set; }
    public bool Enable { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public string SubId { get; set; } = string.Empty;
    public long Up { get; set; }
    public long Down { get; set; }
    public long AllTime { get; set; }
    public long ExpiryTime { get; set; }
    public long Total { get; set; }
    public int Reset { get; set; }
    public long LastOnline { get; set; }
}
