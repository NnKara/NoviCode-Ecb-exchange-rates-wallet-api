using NoviCode.Api.ExceptionHandling;
using NoviCode.Api.Jobs;
using Quartz;

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

        services.AddQuartz(quartz =>
        {
            var jobKey = new JobKey(nameof(ExchangeRatesSyncJob));

            quartz.AddJob<ExchangeRatesSyncJob>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(ExchangeRatesSyncJob)}-trigger")
                .WithSimpleSchedule(s => s
                    .WithInterval(TimeSpan.FromMinutes(1))
                    .RepeatForever()));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
