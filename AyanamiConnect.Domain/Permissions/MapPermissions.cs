using System.Collections.ObjectModel;
using AyanamiConnect.Domain.Enums;

namespace AyanamiConnect.Domain.Permissions;

public static class MapPermissions
{
    public static IReadOnlyDictionary<UserRole, IReadOnlyCollection<string>> Map =
        new ReadOnlyDictionary<UserRole, IReadOnlyCollection<string>>(
            new Dictionary<UserRole, IReadOnlyCollection<string>>
            {
                
            });
}