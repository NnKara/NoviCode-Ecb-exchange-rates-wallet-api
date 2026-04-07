using System.Globalization;
using NoviCode.Application.EcbCurrenciesRate;
using NoviCode.Application.ExchangeRates.Caching;
using NoviCode.Application.ExchangeRates.Helpers;
using NoviCode.Application.ExchangeRates.Interfaces;
using Quartz;

namespace NoviCode.Api.Jobs;

[DisallowConcurrentExecution]
public sealed class ExchangeRatesSyncJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExchangeRatesSyncJob> _logger;

    public ExchangeRatesSyncJob(IServiceScopeFactory scopeFactory, ILogger<ExchangeRatesSyncJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var ecbGateway = scope.ServiceProvider.GetRequiredService<IEcbCurrenciesRate>();
            var bulkWriter = scope.ServiceProvider.GetRequiredService<IExchangeRatesBulkWriter>();
            var cache = scope.ServiceProvider.GetRequiredService<IExchangeRatesCache>();

            var ratesResponse = await ecbGateway.GetLatestRatesAsync(cancellationToken);
            var rows = EcbRatesToExchangeRows.MapToRows(ratesResponse);

            await bulkWriter.MergeAsync(rows, cancellationToken);

            var latestRatesSnapshot = new LatestRatesSnapshotDto
            {
                RateDate = ratesResponse.RateDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                BaseCurrency = ratesResponse.BaseCurrency,
                Rates = ratesResponse.Rates.Select(r => new RateItemDto
                {
                    TargetCurrency = r.Currency,
                    Rate = r.Rate
                }).ToList()
            };

            await cache.SetLatestEurAsync(latestRatesSnapshot, cancellationToken);

            _logger.LogInformation("ECB exchange rates sync completed. {RowCount} row(s) merged.", rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ECB exchange rates sync failed.");
        }
    }
}
