namespace AyanamiConnect.Infrastructure.ThreeXUi.Contracts;

public sealed class InboundResponse
{
    public int Id { get; set; }
    public long Up { get; set; }
    public long Down { get; set; }
    public long Total { get; set; }
    public long AllTime { get; set; }
    public string Remark { get; set; } = string.Empty;
    public bool Enable { get; set; }
    public long ExpiryTime { get; set; }
    public string TrafficReset { get; set; } = string.Empty;
    public long LastTrafficResetTime { get; set; }
    public List<ClientStatResponse>? ClientStats { get; set; }
    public string Listen { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public string Settings { get; set; } = string.Empty;
    public string StreamSettings { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Sniffing { get; set; } = string.Empty;
}
