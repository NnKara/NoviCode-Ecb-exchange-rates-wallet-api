

namespace NoviCode.Application.EcbCurrenciesRate
{
    public sealed class EcbRatesResponse
    {
        public required DateOnly RateDate { get; init; }
        public string BaseCurrency { get; init; } = "EUR";
        public IReadOnlyCollection<EcbRate> Rates { get; init; } = [];
    }
    public sealed class EcbRate
    {
        public required string Currency { get; init; }
        public required decimal Rate { get; init; }
    }
}
