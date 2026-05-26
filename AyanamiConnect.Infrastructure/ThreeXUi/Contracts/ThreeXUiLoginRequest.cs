using System.Text.Json.Serialization;

namespace AyanamiConnect.Infrastructure.ThreeXUi.Contracts;

public sealed record ThreeXUiLoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;
}
