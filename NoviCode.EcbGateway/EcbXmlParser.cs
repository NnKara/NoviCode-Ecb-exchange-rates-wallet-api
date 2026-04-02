using NoviCode.Application.EcbCurrenciesRate;
using NoviCode.Application.Exceptions;
using System.Globalization;
using System.Xml.Linq;

namespace NoviCode.EcbGateway;

public static class EcbXmlParser
{
    public static EcbRatesResponse Parse(string xml)
    {
        XDocument doc;

        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("ECB response is not valid XML.", ex);
        }

        var timeCube = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Cube" && e.Attribute("time") is not null);

        var timeValue = timeCube?.Attribute("time")?.Value;

        if (string.IsNullOrWhiteSpace(timeValue) || !DateOnly.TryParse(timeValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var rateDate))
        {
            throw new ExternalServiceException("ECB XML does not contain a valid cube.");
        }

        var items = new List<EcbRate>();

        foreach (var cube in timeCube!.Elements().Where(e => e.Name.LocalName == "Cube"))
        {
            var currency = cube.Attribute("currency")?.Value;
            var rateRaw = cube.Attribute("rate")?.Value;

            if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(rateRaw))
                continue;

            if (!decimal.TryParse(rateRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
                throw new ExternalServiceException($"Invalid rate for currency '{currency}'.");

            items.Add(new EcbRate
            {
                Currency = currency.Trim().ToUpperInvariant(),
                Rate = rate,
            });
        }

        items.Add(new EcbRate { Currency = "EUR", Rate = 1m });

        return new EcbRatesResponse
        {
            RateDate = rateDate,
            BaseCurrency = "EUR",
            Rates = items,
        };
    }
}
