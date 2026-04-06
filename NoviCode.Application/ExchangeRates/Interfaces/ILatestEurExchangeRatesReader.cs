using NoviCode.Domain.Entities;

namespace NoviCode.Application.ExchangeRates.Interfaces;

public interface ILatestEurExchangeRatesReader
{
    Task<IReadOnlyList<ExchangeRate>> GetLatestEurRatesAsync(CancellationToken cancellationToken = default);
}
