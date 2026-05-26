using AyanamiConnect.Domain;
using AyanamiConnect.Persistence.Entities;

namespace AyanamiConnect.Persistence.Mapper;

public static class UsersMapper
{
    public static UserEntity ToEntity(this User user)
        => new UserEntity
        {
            Id = user.Id,
            TelegramId = user.TelegramId,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            LanguageCode = user.LanguageCode,
            Balance = user.Balance,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LastActiveAt = user.LastActiveAt,
            BalanceOperations = user.BalanceOperations
                .Select(x => x.ToEntity(user.Id))
                .ToList(),
            PanelClients = user.PanelClients
                .Select(x => x.ToEntity(user.Id))
                .ToList(),
            Subscriptions = user.Subscriptions
                .Select(x => x.ToEntity(user.Id))
                .ToList()
        };

    public static User ToDomain(this UserEntity entity)
    {
        var user = User.Load(
            entity.Id,
            entity.TelegramId,
            entity.UserName,
            entity.FirstName,
            entity.LastName,
            entity.LanguageCode,
            entity.Balance,
            entity.Role,
            entity.CreatedAt,
            entity.LastActiveAt);

        if (entity.PanelClients.Count != 0)
        {
            user.PanelClients.AddRange(entity.PanelClients.Select(x => x.ToDomain()));
        }

        if (entity.BalanceOperations.Count != 0)
        {
            user.BalanceOperations.AddRange(entity.BalanceOperations.Select(x => x.ToDomain()));
        }

        if (entity.Subscriptions.Count != 0)
        {
            user.Subscriptions.AddRange(entity.Subscriptions.Select(x => x.ToDomain()));
        }

        return user;
    }
}
