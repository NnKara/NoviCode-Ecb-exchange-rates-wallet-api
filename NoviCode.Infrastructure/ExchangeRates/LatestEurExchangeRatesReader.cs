using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Application.ExchangeRates.DTOs;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.Infrastructure.Data;

namespace NoviCode.Infrastructure.ExchangeRates;

public sealed class LatestEurExchangeRatesReader : ILatestEurExchangeRatesReader
{
    private readonly AppDbContext _db;

    public LatestEurExchangeRatesReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ExchangeRateRow>> GetLatestEurRatesAsync(CancellationToken cancellationToken = default)
    {
        var (_, rows) = await GetLatestEurRatesWithDateAsync(cancellationToken);
        return rows;
    }

    public async Task<(DateOnly LatestDate, IReadOnlyList<ExchangeRateRow> Rows)> GetLatestEurRatesWithDateAsync(CancellationToken cancellationToken = default)
    {
        var anyRatesExist = await _db.ExchangeRates.AnyAsync(cancellationToken);

        if (!anyRatesExist)
            throw new ValidationException("No exchange rates are available.");

        var latestDate = await _db.ExchangeRates.MaxAsync(x => x.RateDate, cancellationToken);

        var rows = await _db.ExchangeRates.AsNoTracking()
            .Where(x => x.RateDate == latestDate && x.BaseCurrency == "EUR")
            .Select(x => new ExchangeRateRow(x.RateDate, x.BaseCurrency, x.TargetCurrency, x.Rate))
            .ToListAsync(cancellationToken);

        return (latestDate, rows);
    }
}
