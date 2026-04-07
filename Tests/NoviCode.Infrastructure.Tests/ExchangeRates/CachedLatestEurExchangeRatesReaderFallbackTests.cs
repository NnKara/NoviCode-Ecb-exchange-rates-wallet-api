using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Application.ExchangeRates.Caching;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.ExchangeRates.Caching;
using Xunit;

namespace NoviCode.Infrastructure.Tests.ExchangeRates;

public sealed class CachedLatestEurExchangeRatesReaderFallbackTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetLatestEurRatesAsync_cache_hit_returns_snapshot_rows_and_does_not_write_cache()
    {
        await using var db = CreateInMemoryDbContext();
        var cache = new FakeCache
        {
            SnapshotToReturn = new LatestRatesSnapshotDto
            {
                RateDate = "2024-06-10",
                BaseCurrency = "EUR",
                Rates = new List<RateItemDto>
                {
                    new() { TargetCurrency = "usd", Rate = 1.1m },
                    new() { TargetCurrency = "gbp", Rate = 0.85m },
                }
            }
        };

        var reader = new CachedLatestEurExchangeRatesReader(db, cache);

        var result = await reader.GetLatestEurRatesAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.TargetCurrency == "USD" && r.Rate == 1.1m);
        Assert.Contains(result, r => r.TargetCurrency == "GBP" && r.Rate == 0.85m);
        Assert.Equal(0, cache.SetCalls);
    }

    [Fact]
    public async Task GetLatestEurRatesAsync_cache_invalid_falls_back_to_db_and_updates_cache()
    {
        await using var db = CreateInMemoryDbContext();
        var latestDate = new DateOnly(2024, 6, 10);
        var now = DateTime.UtcNow;

        db.ExchangeRates.AddRange(
            new ExchangeRate { RateDate = latestDate, BaseCurrency = "EUR", TargetCurrency = "USD", Rate = 1.1m, CreatedAt = now, UpdatedAt = now },
            new ExchangeRate { RateDate = latestDate, BaseCurrency = "EUR", TargetCurrency = "GBP", Rate = 0.85m, CreatedAt = now, UpdatedAt = now }
        );

        await db.SaveChangesAsync();

        var cache = new FakeCache
        {
            SnapshotToReturn = new LatestRatesSnapshotDto
            {
                RateDate = "not-a-date",
                BaseCurrency = "EUR",
                Rates = []
            }
        };

        var reader = new CachedLatestEurExchangeRatesReader(db, cache);

        var result = await reader.GetLatestEurRatesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, cache.SetCalls);
        Assert.NotNull(cache.LastSetSnapshot);
        Assert.Equal("2024-06-10", cache.LastSetSnapshot.RateDate);
        Assert.Equal("EUR", cache.LastSetSnapshot.BaseCurrency);
    }

    [Fact]
    public async Task GetLatestEurRatesAsync_cache_miss_and_db_empty_throws_ValidationException()
    {
        await using var db = CreateInMemoryDbContext();
        var cache = new FakeCache { SnapshotToReturn = null };
        var reader = new CachedLatestEurExchangeRatesReader(db, cache);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => reader.GetLatestEurRatesAsync());
        Assert.Contains("No exchange rates", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeCache : IExchangeRatesCache
    {
        public LatestRatesSnapshotDto? SnapshotToReturn { get; set; }

        public int SetCalls { get; set; }
        public LatestRatesSnapshotDto? LastSetSnapshot { get; set; }

        public Task<LatestRatesSnapshotDto?> GetLatestEurAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(SnapshotToReturn);

        public Task SetLatestEurAsync(LatestRatesSnapshotDto snapshot, CancellationToken cancellationToken = default)
        {
            SetCalls++;
            LastSetSnapshot = snapshot;
            return Task.CompletedTask;
        }
    }
}

