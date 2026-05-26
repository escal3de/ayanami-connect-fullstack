using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Domain;

namespace AyanamiConnect.Application.Mapping;

public static class UsersMapper
{
    public static UserResponse ToResponse(this User user)
        => new UserResponse(
            user.Id,
            user.TelegramId,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.LanguageCode,
            user.Balance,
            user.Role.ToString(),
            user.CreatedAt,
            user.LastActiveAt,
            user.PanelClients.Select(x => x.ToResponse()).ToList(),
            user.Subscriptions.Select(x => x.ToResponse()).ToList());
}