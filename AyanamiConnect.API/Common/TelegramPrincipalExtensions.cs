using System.Security.Claims;

namespace AyanamiConnect.API.Common;

public static class TelegramPrincipalExtensions
{
    public static long? GetTelegramId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(TelegramClaims.TelegramId);
        return long.TryParse(value, out var telegramId) ? telegramId : null;
    }

    public static bool CanAccessTelegramId(this ClaimsPrincipal principal, long telegramId)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (principal.IsInRole("Admin"))
        {
            return true;
        }

        return principal.GetTelegramId() == telegramId;
    }
}
