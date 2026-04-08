using NoviCode.Application.ExchangeRates.DTOs;

namespace NoviCode.Application.ExchangeRates.Interfaces;

public interface ILatestEurExchangeRatesReader
{
    Task<IReadOnlyList<ExchangeRateRow>> GetLatestEurRatesAsync(CancellationToken cancellationToken = default);
}
