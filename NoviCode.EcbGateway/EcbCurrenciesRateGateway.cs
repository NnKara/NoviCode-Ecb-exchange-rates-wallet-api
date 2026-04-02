using Microsoft.Extensions.Options;
using NoviCode.Application.EcbCurrenciesRate;
using NoviCode.Application.Exceptions;

namespace NoviCode.EcbGateway;

public sealed class EcbCurrenciesRateGateway(HttpClient httpClient,IOptions<EcbGatewayOptions> options) : IEcbCurrenciesRate
{
    public async Task<EcbRatesResponse> GetLatestRatesAsync(CancellationToken cancellationToken = default)
    {
        var url = options.Value.DailyRatesUrl;

        if (string.IsNullOrWhiteSpace(url))
            throw new ExternalServiceException("EcbGateway : DailyRatesUrl is not configured.");

        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"ECB returned HTTP {response.StatusCode} {response.ReasonPhrase}.");
        }

        var xml = await response.Content.ReadAsStringAsync(cancellationToken);

        return EcbXmlParser.Parse(xml);
    }
}
