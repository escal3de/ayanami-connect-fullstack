using System.Text.Json.Serialization;

namespace AyanamiConnect.Application.Contracts.ThreeXUi;

public record CreatePanelClientRequest
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("flow")]
    public string Flow { get; init; } = string.Empty;

    [JsonPropertyName("limitIp")]
    public int LimitIp { get; init; }

    [JsonPropertyName("totalGB")]
    public long TotalGB { get; init; }

    [JsonPropertyName("expiryTime")]
    public long ExpiryTime { get; init; }

    [JsonPropertyName("enable")]
    public bool Enable { get; init; }

    [JsonPropertyName("tgId")]
    public string TgId { get; init; } = string.Empty;

    [JsonPropertyName("subId")]
    public string SubId { get; init; } = string.Empty;

    [JsonPropertyName("comment")]
    public string Comment { get; init; } = string.Empty;

    [JsonPropertyName("reset")]
    public int Reset { get; init; }
}
