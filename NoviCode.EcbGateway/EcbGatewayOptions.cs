
namespace NoviCode.EcbGateway
{
    public sealed class EcbGatewayOptions
    {
        public const string SectionName = "EcbGateway";
        public string DailyRatesUrl { get; set; } = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

        public const int DefaultTimeoutSeconds = 30;
        public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;
    }
}
