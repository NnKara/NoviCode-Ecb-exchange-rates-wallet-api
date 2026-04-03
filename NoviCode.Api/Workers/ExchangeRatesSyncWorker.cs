using NoviCode.Application.EcbCurrenciesRate;
using NoviCode.Application.ExchangeRates.Helpers;
using NoviCode.Application.ExchangeRates.Interfaces;

namespace NoviCode.Api.Workers
{
    public class ExchangeRatesSyncWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExchangeRatesSyncWorker> _logger;
        public ExchangeRatesSyncWorker(IServiceScopeFactory scopeFactory,ILogger<ExchangeRatesSyncWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var ecbGateway = scope.ServiceProvider.GetRequiredService<IEcbCurrenciesRate>();
                    var bulkWriter = scope.ServiceProvider.GetRequiredService<IExchangeRatesBulkWriter>();

                    var ratesResponse = await ecbGateway.GetLatestRatesAsync(cancellationToken);
                    var rows = EcbRatesToExchangeRows.MapToRows(ratesResponse);

                    await bulkWriter.MergeAsync(rows, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ECB exchange rates sync failed.");
                }

                try
                {
                    await timer.WaitForNextTickAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
