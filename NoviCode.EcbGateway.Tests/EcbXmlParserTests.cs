using NoviCode.Application.Exceptions;
using Xunit;

namespace NoviCode.EcbGateway.Tests;

public class EcbXmlParserTests
{
    private const string MinimalValidEcbXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <gesmes:Envelope xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01" xmlns="http://www.ecb.int/vocabulary/2002-08-01/eurofxref">
          <Cube>
            <Cube time="2024-04-02">
              <Cube currency="USD" rate="1.0764"/>
              <Cube currency="gbp" rate="0.8531"/>
            </Cube>
          </Cube>
        </gesmes:Envelope>
        """;

    [Fact]
    public void Parse_validXml_returnsDateBaseCurrencyEurAndRatesIncludingSyntheticEur()
    {
        var result = EcbXmlParser.Parse(MinimalValidEcbXml);

        Assert.Equal(new DateOnly(2024, 4, 2), result.RateDate);
        Assert.Equal("EUR", result.BaseCurrency);

        var usd = result.Rates.Single(r => r.Currency == "USD");
        Assert.Equal(1.0764m, usd.Rate);

        var gbp = result.Rates.Single(r => r.Currency == "GBP");
        Assert.Equal(0.8531m, gbp.Rate);

        var eur = result.Rates.Single(r => r.Currency == "EUR");
        Assert.Equal(1m, eur.Rate);
    }

    [Fact]
    public void Parse_skipsCubesWithMissingCurrencyOrRate()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <root xmlns="http://www.ecb.int/vocabulary/2002-08-01/eurofxref">
              <Cube>
                <Cube time="2024-01-15">
                  <Cube currency="CHF" rate="0.94"/>
                  <Cube currency="" rate="1"/>
                  <Cube currency="JPY" rate=""/>
                </Cube>
              </Cube>
            </root>
            """;

        var result = EcbXmlParser.Parse(xml);

        Assert.Single(result.Rates, r => r.Currency == "CHF");
        Assert.Single(result.Rates, r => r.Currency == "EUR");
    }

    [Fact]
    public void Parse_noTimeCube_throwsExternalServiceException()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <root><Cube></Cube></root>
            """;

        var ex = Assert.Throws<ExternalServiceException>(() => EcbXmlParser.Parse(xml));

        Assert.Contains("valid cube", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
