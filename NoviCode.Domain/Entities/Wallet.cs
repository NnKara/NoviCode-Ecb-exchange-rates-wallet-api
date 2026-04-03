using NoviCode.Domain.Exceptions;

namespace NoviCode.Domain.Entities;

public sealed class Wallet
{
    private Wallet() { }

    public long Id { get; private set; }
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = default!;

    public static Wallet Create(string currency, decimal initialBalance = 0)
    {
        if (initialBalance < 0)
            throw new DomainValidationException("Balance cannot be negative.");

        return new Wallet
        {
            Balance = initialBalance,
            Currency = NormalizeCurrencyCode(currency),
        };
    }

    public static string NormalizeCurrencyCode(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainValidationException("Currency is required.");

        var trimmedCurrency = currency.Trim().ToUpperInvariant();

        if (trimmedCurrency.Length != 3 || !trimmedCurrency.All(char.IsLetter))
            throw new DomainValidationException("Currency must be a 3-letter ISO code.");

        return trimmedCurrency;
    }
}