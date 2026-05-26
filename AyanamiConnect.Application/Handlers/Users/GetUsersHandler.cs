using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Application.Mapping;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Handlers.Users;

public class GetUsersHandler(IUsersRepository repository)
{
    private readonly IUsersRepository _repository = repository;

    public async Task<Result<IEnumerable<UserResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        
        if (users is null)
            return Result.Failure<IEnumerable<UserResponse>>("Users list is empty");

        return users.Select(u => u.ToResponse()).ToList();
    }
}