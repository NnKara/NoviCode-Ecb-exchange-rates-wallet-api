using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoviCode.Application.ExchangeRates.Caching;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.ExchangeRates;
using NoviCode.Infrastructure.ExchangeRates.Caching;
using NoviCode.Infrastructure.Repositories;
using NoviCode.Infrastructure.Wallets;
using StackExchange.Redis;

namespace NoviCode.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("ExchangeRateDb")));

        services.AddScoped<IExchangeRatesBulkWriter, ExchangeRatesBulkWriter>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddSingleton<IWalletBalanceAdjustmentStrategyResolver, WalletBalanceAdjustmentStrategyResolver>();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 2;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddSingleton<IExchangeRatesCache, RedisExchangeRatesCache>();
        services.AddScoped<ILatestEurExchangeRatesReader, CachedLatestEurExchangeRatesReader>();

        return services;
    }
}

