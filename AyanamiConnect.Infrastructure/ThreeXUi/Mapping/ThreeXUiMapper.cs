using AyanamiConnect.Application.Contracts.ThreeXUi;
using AyanamiConnect.Domain.Additional;
using AppPanelClientResponse = AyanamiConnect.Application.Contracts.ThreeXUi.PanelClientResponse;
using InfraClientStatResponse = AyanamiConnect.Infrastructure.ThreeXUi.Contracts.ClientStatResponse;
using AyanamiConnect.Infrastructure.ThreeXUi.Contracts;

namespace AyanamiConnect.Infrastructure.ThreeXUi.Mapping;

public static class ThreeXUiMapper
{
    public static CreateInboundRequest ToRequest(this Inbound inbound)
        => new(
            inbound.PanelInboundId,
            inbound.Remark,
            inbound.ServerAddress,
            inbound.Port,
            inbound.Protocol,
            inbound.IsActive,
            inbound.MaxClientsLimit);

    public static ThreeXUiPanelInbound ToApplicationModel(this InboundResponse inbound)
        => new()
        {
            Id = inbound.Id,
            Up = inbound.Up,
            Down = inbound.Down,
            Total = inbound.Total,
            AllTime = inbound.AllTime,
            Remark = inbound.Remark,
            Enable = inbound.Enable,
            ExpiryTime = inbound.ExpiryTime,
            TrafficReset = inbound.TrafficReset,
            LastTrafficResetTime = inbound.LastTrafficResetTime,
            ClientStats = inbound.ClientStats?.Select(x => x.ToApplicationModel()).ToList(),
            Listen = inbound.Listen,
            Port = inbound.Port,
            Protocol = inbound.Protocol,
            Settings = inbound.Settings,
            StreamSettings = inbound.StreamSettings,
            Tag = inbound.Tag,
            Sniffing = inbound.Sniffing
        };

    public static AppPanelClientResponse ToApplicationModel(this InfraClientStatResponse clientStat)
        => new()
        {
            Id = clientStat.Id,
            InboundId = clientStat.InboundId,
            Enable = clientStat.Enable,
            Email = clientStat.Email,
            Uuid = clientStat.Uuid.ToString(),
            SubId = clientStat.SubId,
            Up = clientStat.Up,
            Down = clientStat.Down,
            AllTime = clientStat.AllTime,
            ExpiryTime = clientStat.ExpiryTime,
            Total = clientStat.Total,
            Reset = clientStat.Reset,
            LastOnline = clientStat.LastOnline
        };
}
