using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace NoviCode.Api.DependencyInjection;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers["Retry-After"] = "60";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    title = "Too Many Requests",
                    detail = "Rate limit exceeded. Please try again later."
                }, cancellationToken);

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", context.HttpContext.Connection.RemoteIpAddress);
            };

            options.AddPolicy("wallet-create-sliding-ip", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0
                });
            });

            options.AddPolicy("wallet-adjust-sliding-ip", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0
                });
            });

            options.AddPolicy("wallet-read-sliding-ip", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0
                });
            });
        });

        return services;
    }
}

