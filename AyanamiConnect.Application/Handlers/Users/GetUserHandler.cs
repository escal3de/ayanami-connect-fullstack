using System.Text.RegularExpressions;
using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Application.Mapping;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Handlers.Users;

public class GetUserHandler(IUsersRepository repository)
{
    private readonly IUsersRepository _repository = repository;

    public async Task<Result<UserResponse>> HandleAsync(string value, CancellationToken cancellationToken)
    {
        var result = value switch
        {
            var x when Guid.TryParse(x, out var id) => await _repository.GetByIdAsync(id, cancellationToken),
            var x when long.TryParse(x, out var id) => await _repository.GetByTelegramIdAsync(id, cancellationToken),
            var x when value.Length > 4 && value.Length < 32 && Regex.IsMatch(value, @"^[A-Za-z]+$") =>
                await _repository.GetByUsernameAsync(value, cancellationToken),
            _ => null
        };

        if (result is null)
            return Result.Failure<UserResponse>("User not found");

        return Result.Success(result.ToResponse());
    }
}