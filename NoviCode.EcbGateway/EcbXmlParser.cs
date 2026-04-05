using NoviCode.Application.EcbCurrenciesRate;
using NoviCode.Application.Exceptions;
using System.Globalization;
using System.Xml.Linq;

namespace NoviCode.EcbGateway;

public static class EcbXmlParser
{
    public static EcbRatesResponse Parse(string xml)
    {
        var document = ParseDocument(xml);
        var timeCube = GetTimeCube(document);
        var rateDate = GetRateDate(timeCube);
        var rates = GetRates(timeCube);

        EnsureEuroBaseRate(rates);

        return new EcbRatesResponse
        {
            RateDate = rateDate,
            BaseCurrency = "EUR",
            Rates = rates
        };
    }

    private static XDocument ParseDocument(string xml)
    {
        try
        {
            return XDocument.Parse(xml);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("ECB response is not valid XML.", ex);
        }
    }

    private static XElement GetTimeCube(XDocument document)
    {
        var timeCube = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "Cube" && x.Attribute("time") is not null);

        if (timeCube is null)
            throw new ExternalServiceException("ECB XML does not contain a valid time cube.");

        return timeCube;
    }

    private static DateOnly GetRateDate(XElement timeCube)
    {
        var rawDate = timeCube.Attribute("time")?.Value;

        if (!DateOnly.TryParse(rawDate,CultureInfo.InvariantCulture,DateTimeStyles.None,out var rateDate))
        {
            throw new ExternalServiceException("ECB XML does not contain a valid rate date.");
        }

        return rateDate;
    }

    private static List<EcbRate> GetRates(XElement timeCube)
    {
        var rates = new List<EcbRate>();

        foreach (var rateCube in timeCube.Elements().Where(x => x.Name.LocalName == "Cube"))
        {
            var rate = ParseRate(rateCube);

            if (rate is not null)
                rates.Add(rate);
        }

        return rates;
    }

    private static EcbRate? ParseRate(XElement rateCube)
    {
        var currency = rateCube.Attribute("currency")?.Value;
        var rawRate = rateCube.Attribute("rate")?.Value;

        if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(rawRate))
            return null;

        if (!decimal.TryParse(rawRate, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedRate))
            throw new ExternalServiceException($"Invalid rate for currency '{currency}'.");

        return new EcbRate
        {
            Currency = currency.Trim().ToUpperInvariant(),
            Rate = parsedRate
        };
    }

    private static void EnsureEuroBaseRate(List<EcbRate> rates)
    {
        const string eur = "EUR";

        if (rates.Exists(x => string.Equals(x.Currency, eur, StringComparison.Ordinal)))
            return;

        rates.Add(new EcbRate
        {
            Currency = eur,
            Rate = 1m
        });
    }
}
