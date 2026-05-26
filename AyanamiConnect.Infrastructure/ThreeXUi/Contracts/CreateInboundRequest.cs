namespace AyanamiConnect.Infrastructure.ThreeXUi.Contracts;

public record CreateInboundRequest(
    int PanelInboundId,
    string Remark,
    string ServerAddress,
    int Port,
    string Protocol,
    bool IsActive,
    int MaxClientsLimit);