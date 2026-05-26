using AyanamiConnect.Domain;

namespace AyanamiConnect.Application.Abstractions.EternalServices;

public interface IThreeXUiServices
{
    Task AuthenticateAsync(CancellationToken cancellationToken);
    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken);
}