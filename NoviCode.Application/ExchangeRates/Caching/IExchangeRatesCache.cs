namespace NoviCode.Application.ExchangeRates.Caching;

public interface IExchangeRatesCache
{
    Task<LatestRatesSnapshotDto?> GetLatestEurAsync(CancellationToken cancellationToken = default);
    Task SetLatestEurAsync(LatestRatesSnapshotDto snapshot, CancellationToken cancellationToken = default);
}
