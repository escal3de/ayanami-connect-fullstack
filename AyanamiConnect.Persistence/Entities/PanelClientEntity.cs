namespace AyanamiConnect.Persistence.Entities;

public class PanelClientEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public string SubId { get; set; } = string.Empty;
    public long ExpiryTime { get; set; }
    public long TotalGB { get; set; }
    public int LimitIp { get; set; }
    public string Flow { get; set; } = string.Empty;
    public bool Enable { get; set; }
    public int Reset { get; set; }
}
