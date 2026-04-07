using Microsoft.Extensions.DependencyInjection;
using NoviCode.Application.Wallets;
using NoviCode.Application.Wallets.Interfaces;

namespace NoviCode.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWalletService, WalletService>();

        return services;
    }
}

