
namespace NoviCode.Application.EcbCurrenciesRate
{
    public interface IEcbCurrenciesRate
    {
        Task<EcbRatesResponse> GetLatestRatesAsync(CancellationToken cancellationToken = default);
    }
}
