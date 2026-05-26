using AyanamiConnect.Domain.Additional;
using AyanamiConnect.Persistence.Entities;

namespace AyanamiConnect.Persistence.Mapper;

public static class InboundsMapper
{
    public static InboundEntity ToEntity(this Inbound inbound)
        => new InboundEntity
        {
            Id = inbound.Id,
            PanelInboundId = inbound.PanelInboundId,
            Remark = inbound.Remark,
            ServerAddress = inbound.ServerAddress,
            Port = inbound.Port,
            Protocol = inbound.Protocol,
            IsActive = inbound.IsActive,
            MaxClientsLimit = inbound.MaxClientsLimit,
            CreatedAt = inbound.CreatedAt
        };

    public static Inbound ToDomain(this InboundEntity inbound)
        => Inbound.Load(
            inbound.Id,
            inbound.PanelInboundId,
            inbound.Remark,
            inbound.ServerAddress,
            inbound.Port,
            inbound.Protocol,
            inbound.IsActive,
            inbound.MaxClientsLimit,
            inbound.CreatedAt);
}
