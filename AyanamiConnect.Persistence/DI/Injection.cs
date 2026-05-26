using AyanamiConnect.Application.Abstractions.Repositories;
using AyanamiConnect.Persistence.DbContext;
using AyanamiConnect.Persistence.Realisations.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AyanamiConnect.Persistence.DI;

public static class Injection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUsersRepository, UsersRepository>();
        
        return services;
    }
}