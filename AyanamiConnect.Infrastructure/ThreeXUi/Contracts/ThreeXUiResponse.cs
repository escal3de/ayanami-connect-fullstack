namespace AyanamiConnect.Infrastructure.ThreeXUi.Contracts;

public sealed class ThreeXUiResponse<T>
{
    public bool Success { get; set; }
    public string Msg { get; set; } = string.Empty;
    public T? Obj { get; set; }
}