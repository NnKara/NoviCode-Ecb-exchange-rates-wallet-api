using System.Globalization;
using NoviCode.Application.ExchangeRates.Caching;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.ExchangeRates;

namespace NoviCode.Infrastructure.ExchangeRates.Caching;

public sealed class CachedLatestEurExchangeRatesReader : ILatestEurExchangeRatesReader
{
    private readonly LatestEurExchangeRatesReader _latestFromDb;
    private readonly IExchangeRatesCache _cache;

    public CachedLatestEurExchangeRatesReader(LatestEurExchangeRatesReader latestFromDb, IExchangeRatesCache cache)
    {
        _latestFromDb = latestFromDb;
        _cache = cache;
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetLatestEurRatesAsync(CancellationToken cancellationToken = default)
    {
        var snap = await _cache.GetLatestEurAsync(cancellationToken);

        if (snap is not null && !string.IsNullOrWhiteSpace(snap.RateDate) &&
            DateOnly.TryParse(snap.RateDate, CultureInfo.InvariantCulture, out var rateDate))
        {
            return snap.Rates
                .Where(r => !string.IsNullOrWhiteSpace(r.TargetCurrency))
                .Select(r => new ExchangeRate
                {
                    Id = 0,
                    RateDate = rateDate,
                    BaseCurrency = snap.BaseCurrency,
                    TargetCurrency = r.TargetCurrency!.Trim().ToUpperInvariant(),
                    Rate = r.Rate,
                    CreatedAt = default,
                    UpdatedAt = default
                }).ToList();
        }

        var (latestDate, rows) = await _latestFromDb.GetLatestEurRatesWithDateAsync(cancellationToken);

        var snapshotFromDb = new LatestRatesSnapshotDto
        {
            RateDate = latestDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            BaseCurrency = "EUR",
            Rates = rows.Select(x => new RateItemDto
            {
                TargetCurrency = x.TargetCurrency,
                Rate = x.Rate
            }).ToList()
        };

        await _cache.SetLatestEurAsync(snapshotFromDb, cancellationToken);

        return rows;
    }
}
