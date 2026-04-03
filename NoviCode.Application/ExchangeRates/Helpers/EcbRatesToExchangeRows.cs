using NoviCode.Application.EcbCurrenciesRate;
using NoviCode.Application.ExchangeRates.DTOs;

namespace NoviCode.Application.ExchangeRates.Helpers;

public static class EcbRatesToExchangeRows
{
    public static IReadOnlyList<ExchangeRateRow> MapToRows(EcbRatesResponse response)
    {
        var ratesList = new List<ExchangeRateRow>();

        foreach (var rate in response.Rates)
        {
            ratesList.Add(new ExchangeRateRow(
                response.RateDate,
                response.BaseCurrency,
                rate.Currency,
                rate.Rate));
        }

        return ratesList;
    }
}
