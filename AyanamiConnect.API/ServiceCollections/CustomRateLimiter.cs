using System.Threading.RateLimiting;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.API.ServiceCollections;

public static class CustomRateLimiter
{
    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context
                => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name
                                  ?? context.Connection.RemoteIpAddress?.ToString()
                                  ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 1000,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.OnRejected = async (context, token) =>
            {
                var response = context.HttpContext.Response;
                response.Headers["Retry-After"] = "60";
                response.ContentType = "application/json";
                response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                await response.WriteAsJsonAsync("""{"error": "Too many requests!"}""");
            };
        });

        return services;
    }
}