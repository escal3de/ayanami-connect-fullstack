using AyanamiConnect.Domain.Additional;
using AyanamiConnect.Application.Contracts.ThreeXUi;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Abstractions.EternalServices;

public interface IThreeXUiInboundsService
{
    Task<Result<IEnumerable<ThreeXUiPanelInbound>>> GetInboundsAsync(CancellationToken cancellationToken);
    Task<Result<ThreeXUiPanelInbound>> GetInboundAsync(int panelInboundId, CancellationToken cancellationToken);
    Task<Result> CreateInboundAsync(Inbound inbound, CancellationToken cancellationToken);
    Task<Result> DeleteInboundAsync(int panelInboundId, CancellationToken cancellationToken);
}
