using System.Text.Json;
using Microsoft.Extensions.Logging;
using NoviCode.Application.ExchangeRates.Caching;
using StackExchange.Redis;

namespace NoviCode.Infrastructure.ExchangeRates.Caching;

public sealed class RedisExchangeRatesCache : IExchangeRatesCache
{
    private readonly IDatabase _redisDb;
    private readonly ILogger<RedisExchangeRatesCache> _logger;

    public RedisExchangeRatesCache(IConnectionMultiplexer redis, ILogger<RedisExchangeRatesCache> logger)
    {
        _redisDb = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<LatestRatesSnapshotDto?> GetLatestEurAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedJson = await _redisDb.StringGetAsync(ExchangeRatesCacheKeys.LatestEur);

            if (cachedJson.IsNullOrEmpty)
                return null;

            var snapshot = JsonSerializer.Deserialize<LatestRatesSnapshotDto>((string)cachedJson!);

            if (snapshot is null || string.IsNullOrWhiteSpace(snapshot.RateDate))
            {
                _logger.LogWarning("Redis EUR snapshot missing RateDate or failed deserialization! Falling back to database.");
                return null;
            }

            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis read failed! Falling back to database.");
            return null;
        }
    }

    public async Task SetLatestEurAsync(LatestRatesSnapshotDto snapshot, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(snapshot);
            await _redisDb.StringSetAsync(ExchangeRatesCacheKeys.LatestEur, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update Redis cache after write");
        }
    }
}
