namespace NoviCode.Application.ExchangeRates.DTOs;

public sealed record ExchangeRateRow(DateOnly RateDate,string BaseCurrency,string TargetCurrency,decimal Rate);
