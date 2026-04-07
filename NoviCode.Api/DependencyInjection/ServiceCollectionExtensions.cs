using Microsoft.Extensions.DependencyInjection;
using NoviCode.Api.ExceptionHandling;
using NoviCode.Api.Workers;

namespace NoviCode.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHostedService<ExchangeRatesSyncWorker>();

        return services;
    }
}

