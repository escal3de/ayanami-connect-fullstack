using System.Net.Http.Json;
using AyanamiConnect.Application.Abstractions.EternalServices;
using AyanamiConnect.Infrastructure.ThreeXUi.Contracts;
using AyanamiConnect.Infrastructure.ThreeXUi.Options;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;

namespace AyanamiConnect.Infrastructure.ThreeXUi.Services;

public sealed class ThreeXUiAuthService(HttpClient client, IOptions<ThreeXUiOptions> options) : IThreeXUiAuthService
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(11);

    private readonly HttpClient _httpClient = client;
    private readonly ThreeXUiOptions _options = options.Value;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private DateTimeOffset? _authenticatedAtUtc;

    public async Task<Result> EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (IsSessionFresh())
            return Result.Success();

        await _sync.WaitAsync(cancellationToken);

        try
        {
            if (IsSessionFresh())
                return Result.Success();

            var loginResult = await LoginAsync(cancellationToken);
            if (loginResult.IsFailure)
                return loginResult;

            _authenticatedAtUtc = DateTimeOffset.UtcNow;
            return Result.Success();
        }
        finally
        {
            _sync.Release();
        }
    }

    public void Invalidate()
    {
        _authenticatedAtUtc = null;
    }

    private bool IsSessionFresh()
        => _authenticatedAtUtc is not null && DateTimeOffset.UtcNow - _authenticatedAtUtc < SessionTtl;

    private async Task<Result> LoginAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "login",
                new ThreeXUiLoginRequest
                {
                    Username = _options.Username,
                    Password = _options.Password
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Result.Failure($"3X-UI login failed with status code {response.StatusCode}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"3X-UI login failed: {ex.Message}");
        }
    }
}
