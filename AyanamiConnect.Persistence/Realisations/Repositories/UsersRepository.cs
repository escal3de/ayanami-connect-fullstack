using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Domain;
using AyanamiConnect.Persistence.DbContext;
using AyanamiConnect.Persistence.Entities;
using AyanamiConnect.Persistence.Mapper;
using Microsoft.EntityFrameworkCore;

namespace AyanamiConnect.Persistence.Realisations.Repositories;

public class UsersRepository(UsersDbContext dbContext) : IUsersRepository
{
    private readonly UsersDbContext _dbContext = dbContext;

    private IQueryable<UserEntity> QueryWithRelations()
        => _dbContext.Users
            .AsNoTracking()
            .Include(x => x.BalanceOperations)
            .Include(x => x.PanelClients)
            .Include(x => x.Subscriptions);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await QueryWithRelations()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken)
    {
        var entity = await QueryWithRelations()
            .FirstOrDefaultAsync(x => x.TelegramId == telegramId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var entity = await QueryWithRelations()
            .FirstOrDefaultAsync(x => x.UserName == username, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await QueryWithRelations().ToListAsync(cancellationToken);
        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<User>> GetAllByTelegramIdAsync(long telegramId, CancellationToken cancellationToken)
    {
        var entities = await QueryWithRelations()
            .Where(x => x.TelegramId == telegramId)
            .ToListAsync(cancellationToken);

        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user.ToEntity(), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        var entity = user.ToEntity();

        _dbContext.Users.Update(entity);

        var existingBalanceOperationIds = await _dbContext.BalanceOperations
            .AsNoTracking()
            .Where(x => x.UserId == entity.Id)
            .Select(x => x.Id)
            .ToHashSetAsync(cancellationToken);

        foreach (var balanceOperation in entity.BalanceOperations)
        {
            _dbContext.Entry(balanceOperation).State = existingBalanceOperationIds.Contains(balanceOperation.Id)
                ? EntityState.Unchanged
                : EntityState.Added;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Remove(user.ToEntity());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
