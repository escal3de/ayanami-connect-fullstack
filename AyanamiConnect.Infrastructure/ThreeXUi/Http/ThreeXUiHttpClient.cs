using System.Net;
using System.Net.Http.Json;
using AyanamiConnect.Application.Abstractions.EternalServices;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Infrastructure.ThreeXUi.Http;

public class ThreeXUiHttpClient(HttpClient client, IThreeXUiAuthService authService) : IThreeXUiClient
{
    private readonly HttpClient _httpClient = client;
    private readonly IThreeXUiAuthService _authService = authService;

    public async Task<Result<TResponse?>> GetAsync<TResponse>(string relativeUrl, CancellationToken cancellationToken)
    {
        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure<TResponse?>(authResult.Error);

        var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
        if (IsAuthFailure(response))
            return await RetryAsync<TResponse>(relativeUrl, cancellationToken, () => _httpClient.GetAsync(relativeUrl, cancellationToken), "GET");

        if (!response.IsSuccessStatusCode)
            return Result.Failure<TResponse?>($"GET {relativeUrl} failed with status code {response.StatusCode}");

        return await ReadResponseAsync<TResponse>(response);
    }

    public async Task<Result<TResponse?>> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest body,
        CancellationToken cancellationToken)
    {
        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure<TResponse?>(authResult.Error);

        var response = await _httpClient.PostAsJsonAsync(relativeUrl, body, cancellationToken);
        if (IsAuthFailure(response))
            return await RetryAsync<TResponse>(relativeUrl, cancellationToken, () => _httpClient.PostAsJsonAsync(relativeUrl, body, cancellationToken), "POST");

        if (!response.IsSuccessStatusCode)
            return Result.Failure<TResponse?>($"POST {relativeUrl} failed with status code {response.StatusCode}");

        return await ReadResponseAsync<TResponse>(response);
    }

    public async Task<Result> PostAsync<TRequest>(string relativeUrl, TRequest body, CancellationToken cancellationToken)
    {
        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        var response = await _httpClient.PostAsJsonAsync(relativeUrl, body, cancellationToken);
        if (IsAuthFailure(response))
            return await RetryPostWithoutResponseAsync(relativeUrl, body, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure($"POST {relativeUrl} failed with status code {response.StatusCode}");

        return Result.Success();
    }

    public async Task<Result> PostAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        var response = await _httpClient.PostAsync(relativeUrl, null, cancellationToken);
        if (IsAuthFailure(response))
            return await RetryPostWithoutResponseAsync(relativeUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure($"POST {relativeUrl} failed with status code {response.StatusCode}");

        return Result.Success();
    }

    public async Task<Result> PostFormAsync(string relativeUrl, IReadOnlyDictionary<string, string> formData, CancellationToken cancellationToken)
    {
        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        using var content = new FormUrlEncodedContent(formData);
        var response = await _httpClient.PostAsync(relativeUrl, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);
        if (IsAuthFailure(response))
            return await RetryPostFormWithoutResponseAsync(relativeUrl, formData, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure($"POST {relativeUrl} failed with status code {response.StatusCode}");

        return Result.Success();
    }

    public async Task<Result<TResponse?>> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest body,
        CancellationToken cancellationToken)
    {
        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure<TResponse?>(authResult.Error);

        var response = await _httpClient.PutAsJsonAsync(relativeUrl, body, cancellationToken);
        if (IsAuthFailure(response))
            return await RetryAsync<TResponse>(relativeUrl, cancellationToken, () => _httpClient.PutAsJsonAsync(relativeUrl, body, cancellationToken), "PUT");

        if (!response.IsSuccessStatusCode)
            return Result.Failure<TResponse?>($"PUT {relativeUrl} failed with status code {response.StatusCode}");

        return await ReadResponseAsync<TResponse>(response);
    }

    public async Task<Result> DeleteAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        var response = await _httpClient.DeleteAsync(relativeUrl, cancellationToken);
        if (IsAuthFailure(response))
            return await RetryDeleteAsync(relativeUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure($"DELETE {relativeUrl} failed with status code {response.StatusCode}");

        return Result.Success();
    }

    private static bool IsAuthFailure(HttpResponseMessage response)
        => response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

    private static async Task<Result<TResponse?>> ReadResponseAsync<TResponse>(HttpResponseMessage response)
        => Result.Success(await response.Content.ReadFromJsonAsync<TResponse>());

    private async Task<Result<TResponse?>> RetryAsync<TResponse>(
        string relativeUrl,
        CancellationToken cancellationToken,
        Func<Task<HttpResponseMessage>> requestFactory,
        string method)
    {
        _authService.Invalidate();

        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure<TResponse?>(authResult.Error);

        var retryResponse = await requestFactory();
        if (!retryResponse.IsSuccessStatusCode)
            return Result.Failure<TResponse?>($"{method} {relativeUrl} failed with status code {retryResponse.StatusCode}");

        return await ReadResponseAsync<TResponse>(retryResponse);
    }

    private async Task<Result> RetryDeleteAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        _authService.Invalidate();

        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        var retryResponse = await _httpClient.DeleteAsync(relativeUrl, cancellationToken);
        if (!retryResponse.IsSuccessStatusCode)
            return Result.Failure($"DELETE {relativeUrl} failed with status code {retryResponse.StatusCode}");

        return Result.Success();
    }

    private async Task<Result> RetryPostWithoutResponseAsync<TRequest>(
        string relativeUrl,
        TRequest body,
        CancellationToken cancellationToken)
    {
        _authService.Invalidate();

        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        var retryResponse = await _httpClient.PostAsJsonAsync(relativeUrl, body, cancellationToken);
        if (!retryResponse.IsSuccessStatusCode)
            return Result.Failure($"POST {relativeUrl} failed with status code {retryResponse.StatusCode}");

        return Result.Success();
    }

    private async Task<Result> RetryPostWithoutResponseAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        _authService.Invalidate();

        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        var retryResponse = await _httpClient.PostAsync(relativeUrl, null, cancellationToken);
        if (!retryResponse.IsSuccessStatusCode)
            return Result.Failure($"POST {relativeUrl} failed with status code {retryResponse.StatusCode}");

        return Result.Success();
    }

    private async Task<Result> RetryPostFormWithoutResponseAsync(
        string relativeUrl,
        IReadOnlyDictionary<string, string> formData,
        CancellationToken cancellationToken)
    {
        _authService.Invalidate();

        var authResult = await _authService.EnsureAuthenticatedAsync(cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure(authResult.Error);

        using var content = new FormUrlEncodedContent(formData);
        var retryResponse = await _httpClient.PostAsync(relativeUrl, content, cancellationToken);
        if (!retryResponse.IsSuccessStatusCode)
            return Result.Failure($"POST {relativeUrl} failed with status code {retryResponse.StatusCode}");

        return Result.Success();
    }
}
