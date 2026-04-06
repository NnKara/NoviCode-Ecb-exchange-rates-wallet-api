namespace NoviCode.Application.ExchangeRates.Caching;

public sealed class LatestRatesSnapshotDto
{
    public string? RateDate { get; init; }
    public string BaseCurrency { get; init; } = "EUR";
    public List<RateItemDto> Rates { get; init; } = [];
}

public sealed class RateItemDto
{
    public string? TargetCurrency { get; init; } 
    public decimal Rate { get; init; }
}