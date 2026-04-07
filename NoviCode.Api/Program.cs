using Microsoft.EntityFrameworkCore;
using NoviCode.Api.ExceptionHandling;
using NoviCode.Api.Workers;
using NoviCode.Application.ExchangeRates.Caching;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.Application.Wallets;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.EcbGateway;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.ExchangeRates;
using NoviCode.Infrastructure.ExchangeRates.Caching;
using NoviCode.Infrastructure.Repositories;
using NoviCode.Infrastructure.Wallets;
using StackExchange.Redis;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEcbGateway(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExchangeRateDb")));

builder.Services.AddScoped<IExchangeRatesBulkWriter, ExchangeRatesBulkWriter>();
builder.Services.AddHostedService<ExchangeRatesSyncWorker>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddSingleton<IWalletBalanceAdjustmentStrategyResolver, WalletBalanceAdjustmentStrategyResolver>();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IExchangeRatesCache, RedisExchangeRatesCache>();

builder.Services.AddScoped<ILatestEurExchangeRatesReader, CachedLatestEurExchangeRatesReader>();


builder.Services.AddRateLimiter(options =>
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

        logger.LogWarning("Rate limit exceeded for IP: {IpAddress}",context.HttpContext.Connection.RemoteIpAddress);
    };

    options.AddPolicy("wallet-create-sliding-ip", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(ip,_ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0
        });
    });

    options.AddPolicy("wallet-adjust-sliding-ip", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(ip,_ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0
        });
    });

    options.AddPolicy("wallet-read-sliding-ip", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(ip,_ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapControllers();

app.Run();
