using AyanamiConnect.Application.Abstractions.EternalServices;
using AyanamiConnect.Infrastructure.ThreeXUi.Http;
using AyanamiConnect.Infrastructure.ThreeXUi.Options;
using AyanamiConnect.Infrastructure.ThreeXUi.Services;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AyanamiConnect.Infrastructure.DI;

public static class Injection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ThreeXUiOptions>(configuration.GetRequiredSection("ThreeXUi"));
        services.AddSingleton<CookieContainer>();

        services.AddHttpClient<IThreeXUiAuthService, ThreeXUiAuthService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ThreeXUiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl + options.WebBasePath + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .ConfigurePrimaryHttpMessageHandler(CreateHandler);

        services.AddHttpClient<IThreeXUiClient, ThreeXUiHttpClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ThreeXUiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl + options.WebBasePath + options.PanelPath + options.ApiPath + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .ConfigurePrimaryHttpMessageHandler(CreateHandler);

        services.AddScoped<IThreeXUiInboundsService, ThreeXUiInboundsService>();
        services.AddScoped<IThreeXUiClientsService, ThreeXUiClientsService>();

        return services;
    }

    private static SocketsHttpHandler CreateHandler(IServiceProvider sp)
        => new()
        {
            UseCookies = true,
            CookieContainer = sp.GetRequiredService<CookieContainer>(),
            AutomaticDecompression = DecompressionMethods.All
        };
}
