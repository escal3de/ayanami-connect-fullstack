namespace AyanamiConnect.Infrastructure.ThreeXUi.Options;

public class ThreeXUiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string WebBasePath { get; set; } = string.Empty;
    public string PanelPath { get; set; } = string.Empty;
    public string ApiPath { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}