using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Application.ExchangeRates.Caching;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;

namespace NoviCode.Infrastructure.ExchangeRates;

public sealed class CachedLatestEurExchangeRatesReader : ILatestEurExchangeRatesReader
{
    private readonly AppDbContext _db;
    private readonly IExchangeRatesCache _cache;

    public CachedLatestEurExchangeRatesReader(AppDbContext db, IExchangeRatesCache cache)
    {
        _db = db;
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

        var anyRatesExist = await _db.ExchangeRates.AnyAsync(cancellationToken);

        if (!anyRatesExist)
            throw new ValidationException("No exchange rates are available.");

        var latestDate = await _db.ExchangeRates.MaxAsync(x => x.RateDate, cancellationToken);

        var rows = await _db.ExchangeRates.AsNoTracking()
            .Where(x => x.RateDate == latestDate && x.BaseCurrency == "EUR")
            .ToListAsync(cancellationToken);

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
