using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Abstractions.EternalServices;

public interface IThreeXUiClient
{
    Task<Result<TResponse?>> GetAsync<TResponse>(string relativeUrl, CancellationToken cancellationToken);
    Task<Result<TResponse?>> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest body, CancellationToken cancellationToken);
    Task<Result> PostAsync<TRequest>(string relativeUrl, TRequest body, CancellationToken cancellationToken);
    Task<Result> PostAsync(string relativeUrl, CancellationToken cancellationToken);
    Task<Result> PostFormAsync(string relativeUrl, IReadOnlyDictionary<string, string> formData, CancellationToken cancellationToken);
    Task<Result<TResponse?>> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest body, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(string relativeUrl, CancellationToken cancellationToken);
}
