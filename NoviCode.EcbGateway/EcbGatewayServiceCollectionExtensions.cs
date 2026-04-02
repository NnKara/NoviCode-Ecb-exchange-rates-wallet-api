using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NoviCode.Application.EcbCurrenciesRate;
using Polly;
using Polly.Extensions.Http;

namespace NoviCode.EcbGateway;

public static class EcbGatewayServiceCollectionExtensions
{
    public static IServiceCollection AddEcbGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EcbGatewayOptions>(
            configuration.GetSection(EcbGatewayOptions.SectionName));

        services.AddHttpClient<IEcbCurrenciesRate, EcbCurrenciesRateGateway>((sp, client) =>
        {
            var o = sp.GetRequiredService<IOptions<EcbGatewayOptions>>().Value;
            var seconds = o.TimeoutSeconds > 0 ? o.TimeoutSeconds : EcbGatewayOptions.DefaultTimeoutSeconds;
            client.Timeout = TimeSpan.FromSeconds(seconds);
        })
        .AddPolicyHandler((sp, _) => CreateRetryPolicy(sp.GetRequiredService<IOptions<EcbGatewayOptions>>().Value));

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(EcbGatewayOptions options)
    {
        var retryCount = Math.Max(0, options.RetryCount);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
