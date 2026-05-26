using AyanamiConnect.Application.Contracts.Users;
using AyanamiConnect.Application.Handlers.BalanceOperations;
using AyanamiConnect.Application.Handlers.ForAdmin;
using AyanamiConnect.Application.Handlers.Subscriptions;
using AyanamiConnect.Application.Handlers.Users;
using AyanamiConnect.Application.Validators.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AyanamiConnect.Application.DI;

public static class Injection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // validators
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        
        // handlers (users)
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        
        // handlers (subscription)
        services.AddScoped<ExtendSubscriptionHandler>();
        
        // handlers (balance)
        services.AddScoped<AddToBalanceHandler>();
        services.AddScoped<GetBalanceOperationsHandler>();
        
        // handlers (for admin)
        services.AddScoped<AdminExtendSubscriptionHandler>();
        
        return services;
    }
}
