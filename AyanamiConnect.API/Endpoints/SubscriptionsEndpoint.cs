using AyanamiConnect.Application.Handlers.Subscriptions;
using AyanamiConnect.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AyanamiConnect.API.Endpoints;

public static class SubscriptionsEndpoint
{
    public static IEndpointRouteBuilder MapSubscriptionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/subscriptions");

        group.MapPost("/{telegramId:long}/extend/{plan}", async (
            long telegramId,
            string plan,
            ClaimsPrincipal principal,
            [FromServices] ExtendSubscriptionHandler handler,
            CancellationToken cancellationToken = default) =>
        {
            if (!Enum.TryParse<SubscriptionPlans>(plan, ignoreCase: true, out var parsedPlan))
                return Results.BadRequest($"Invalid plan {plan}");

            var result = await handler.HandleAsync(telegramId, parsedPlan, cancellationToken);
            
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        }).WithSummary("Продление подписки")
        .AllowAnonymous();

        return group;
    }
}
