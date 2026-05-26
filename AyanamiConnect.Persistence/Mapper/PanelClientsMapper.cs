using AyanamiConnect.Domain;
using AyanamiConnect.Persistence.Entities;

namespace AyanamiConnect.Persistence.Mapper;

public static class PanelClientsMapper
{
    public static PanelClientEntity ToEntity(this PanelClient panelClient, Guid userId)
        => new PanelClientEntity
        {
            Id = panelClient.Id,
            UserId = userId,
            Email = panelClient.Email,
            Uuid = panelClient.Uuid,
            SubId = panelClient.SubId,
            ExpiryTime = panelClient.ExpiryTime,
            TotalGB = panelClient.TotalGB,
            LimitIp = panelClient.LimitIp,
            Flow = panelClient.Flow,
            Enable = panelClient.Enable,
            Reset = panelClient.Reset
        };

    public static PanelClient ToDomain(this PanelClientEntity entity)
        => PanelClient.Load(
            entity.Id,
            entity.Email,
            entity.Uuid,
            entity.SubId,
            entity.ExpiryTime,
            entity.TotalGB,
            entity.LimitIp,
            entity.Flow,
            entity.Enable,
            entity.Reset);
}
