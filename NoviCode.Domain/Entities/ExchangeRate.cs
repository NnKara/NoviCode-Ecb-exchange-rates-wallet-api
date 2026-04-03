

namespace NoviCode.Domain.Entities
{
    public sealed class ExchangeRate
    {
        public long Id { get; set; }
        public DateOnly RateDate { get; set; }
        public string BaseCurrency { get; set; } = default!;
        public string TargetCurrency { get; set; } = default!;
        public decimal Rate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
