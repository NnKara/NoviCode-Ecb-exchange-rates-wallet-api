using NoviCode.Application.ExchangeRates.DTOs;

namespace NoviCode.Application.ExchangeRates.Interfaces;

public interface IExchangeRatesBulkWriter
{
    Task MergeAsync(IReadOnlyList<ExchangeRateRow> rows, CancellationToken cancellationToken = default);
}
