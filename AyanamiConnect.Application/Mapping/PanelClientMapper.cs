using AyanamiConnect.Application.Contracts.PanelClients;
using AyanamiConnect.Domain;

namespace AyanamiConnect.Application.Mapping;

public static class PanelClientMapper
{
    public static PanelClientResponse ToResponse(this PanelClient panelClient)
        => new PanelClientResponse(
            panelClient.Id,
            panelClient.Email,
            panelClient.Uuid,
            panelClient.SubId,
            panelClient.ExpiryTime,
            panelClient.TotalGB,
            panelClient.LimitIp,
            panelClient.Flow,
            panelClient.Enable,
            panelClient.Reset);
}