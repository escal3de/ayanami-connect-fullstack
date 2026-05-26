using AyanamiConnect.API.Common;
using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Application.Handlers.BalanceOperations;
using AyanamiConnect.Application.Handlers.Users;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AyanamiConnect.API.Endpoints;

public static class UsersEndpoint // ВЕЗДЕ ДОБАВИТЬ AdminOnly Policy!!!!!!!!!!
{
    public static IEndpointRouteBuilder MapUsersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users");

        group.MapGet("/",
            async ([FromServices] GetUsersHandler handler, CancellationToken cancellationToken = default) =>
            {
                var result = await handler.HandleAsync(cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            }).WithSummary("Получение всех пользователей")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/{value}",
            async (
                string value,
                [FromServices] GetUserHandler handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.HandleAsync(value, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            }).WithSummary("Получение пользователя по каким-либо данным (ID, TelegramID, Username)")
            .AllowAnonymous();

        group.MapGet("/{telegramId:long}/operations",
            async (
                long telegramId,
                ClaimsPrincipal principal,
                [FromServices] GetBalanceOperationsHandler handler,
                CancellationToken cancellationToken = default) =>
            {
                if (principal?.Identity?.IsAuthenticated == true) 
                {
                    if (!principal.CanAccessTelegramId(telegramId))
                        return Results.Forbid();
                }

                var result = await handler.HandleAsync(telegramId, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            }).WithSummary("Получение истории операций пользователя")
            .AllowAnonymous();

        group.MapPost("/",
            async (
                CreateUserRequest request,
                ClaimsPrincipal principal,
                [FromServices] CreateUserHandler handler,
                CancellationToken cancellationToken = default) =>
            {
                if (principal?.Identity?.IsAuthenticated == true) 
                {
                    if (!principal.CanAccessTelegramId(request.TelegramId))
                        return Results.Forbid();
                }

                var result = await handler.HandleAsync(request, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(new { success = true })
                    : Results.BadRequest(new { success = false, error = result.Error });
            }).WithSummary("Создание пользователя")
            .AllowAnonymous();

        group.MapDelete("/",
            async (
                long telegramId,
                [FromServices] DeleteUserHandler handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.HandleAsync(telegramId, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(new { success = true })
                    : Results.BadRequest(new { success = false, error = result.Error });
            }).WithSummary("Удаление пользователя")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/{telegramId:long}/addToBalance/{amount:decimal}", async (
            long telegramId,
            decimal amount,
            [FromServices] AddToBalanceHandler handler,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.HandleAsync(telegramId, amount, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        }).WithSummary("Пополнение баланса")
        .RequireAuthorization("AdminOnly");

        return group;
    }
}
