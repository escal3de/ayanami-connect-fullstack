using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Abstractions.EternalServices;

public interface IThreeXUiAuthService
{
    Task<Result> EnsureAuthenticatedAsync(CancellationToken cancellationToken);
    void Invalidate();
}
