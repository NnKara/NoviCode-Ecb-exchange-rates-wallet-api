using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Application.ExchangeRates.Interfaces;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;

namespace NoviCode.Infrastructure.ExchangeRates;

public sealed class LatestEurExchangeRatesReader : ILatestEurExchangeRatesReader
{
    private readonly AppDbContext _db;

    public LatestEurExchangeRatesReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetLatestEurRatesAsync(CancellationToken cancellationToken = default)
    {
        var anyRatesExist = await _db.ExchangeRates.AnyAsync(cancellationToken);

        if (!anyRatesExist)
            throw new ValidationException("No exchange rates are available.");

        var latestDate = await _db.ExchangeRates.MaxAsync(x => x.RateDate, cancellationToken);

        return await _db.ExchangeRates.AsNoTracking()
            .Where(x => x.RateDate == latestDate && x.BaseCurrency == "EUR")
            .ToListAsync(cancellationToken);
    }
}
