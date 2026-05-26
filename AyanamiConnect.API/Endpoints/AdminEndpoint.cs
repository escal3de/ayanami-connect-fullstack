using AyanamiConnect.Application.Handlers.ForAdmin;
using AyanamiConnect.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AyanamiConnect.API.Endpoints;

public static class AdminEndpoint
{
    public static IEndpointRouteBuilder MapAdminEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin");

        group.MapPost("/{telegramId:long}/extend/{plan}", async (
                long telegramId,
                string plan,
                [FromServices] AdminExtendSubscriptionHandler handler,
                CancellationToken cancellationToken = default) =>
            {
                if (!Enum.TryParse<SubscriptionPlans>(plan, ignoreCase: true, out var parsedPlan))
                    return Results.BadRequest($"Invalid plan {plan}");

                var result = await handler.HandleAsync(telegramId, parsedPlan, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            }).WithSummary("Продление подписки (для админа)")
            .RequireAuthorization("AdminOnly");

        return group;
    }
}