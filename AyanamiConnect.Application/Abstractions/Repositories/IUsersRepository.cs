using AyanamiConnect.Domain;

namespace AyanamiConnect.Application.Abstractions.Repositories;

public interface IUsersRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> GetAllByTelegramIdAsync(long telegramId, CancellationToken cancellationToken);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<IEnumerable<User?>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
    Task DeleteAsync(User user, CancellationToken cancellationToken);
}
