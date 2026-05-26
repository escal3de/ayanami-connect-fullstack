using AyanamiConnect.Application.Contracts.ThreeXUi;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Abstractions.EternalServices;

public interface IThreeXUiClientsService
{
    Task<Result<PanelClientResponse>> GetByIdAsync(int clientId, CancellationToken cancellationToken);
    Task<Result<PanelClientResponse>> GetByUuidAsync(string clientUuid, CancellationToken cancellationToken);
    Task<Result<PanelClientResponse>> GetBySubIdAsync(string subId, CancellationToken cancellationToken);
    Task<Result<IEnumerable<PanelClientResponse>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result> CreateClientAsync(CreatePanelClientRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteClientByIdAsync(int clientId, CancellationToken cancellationToken);
    Task<Result> DeleteClientByUuidAsync(string clientUuid, CancellationToken cancellationToken);
    Task<Result> DeleteClientBySubIdAsync(string subId, CancellationToken cancellationToken);
    Task<Result> DeleteClientByEmailAsync(string email, CancellationToken cancellationToken);
}
