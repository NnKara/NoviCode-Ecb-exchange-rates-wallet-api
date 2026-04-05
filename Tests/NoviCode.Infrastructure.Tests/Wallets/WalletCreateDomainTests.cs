using NoviCode.Domain.Entities;
using NoviCode.Domain.Exceptions;
using Xunit;

namespace NoviCode.Infrastructure.Tests.Wallets;

public sealed class WalletCreateDomainTests
{
    [Fact]
    public void Create_with_valid_currency_normalizes_to_uppercase_and_sets_balance()
    {
        var wallet = Wallet.Create("eur", 100m);

        Assert.Equal("EUR", wallet.Currency);
        Assert.Equal(100m, wallet.Balance);
    }

    [Fact]
    public void Create_with_zero_initial_balance_succeeds()
    {
        var wallet = Wallet.Create("USD", 0m);

        Assert.Equal("USD", wallet.Currency);
        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public void Create_with_negative_balance_throws_DomainValidationException()
    {
        var ex = Assert.Throws<DomainValidationException>(() => Wallet.Create("EUR", -200m));

        Assert.Contains("negative", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_with_empty_currency_throws_DomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => Wallet.Create("  ", 0m));
    }

    [Fact]
    public void Create_with_invalid_currency_length_throws_DomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => Wallet.Create("US", 0m));
    }

    [Fact]
    public void SubtractFunds_reduces_balance()
    {
        var wallet = Wallet.Create("EUR", 100m);

        wallet.SubtractFunds(30m);

        Assert.Equal(70m, wallet.Balance);
    }

    [Fact]
    public void SubtractFunds_when_insufficient_throws_DomainValidationException()
    {
        var wallet = Wallet.Create("EUR", 10m);

        var ex = Assert.Throws<DomainValidationException>(() => wallet.SubtractFunds(20m));

        Assert.Contains("Insufficient", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SubtractFunds_with_non_positive_amount_throws_DomainValidationException()
    {
        var wallet = Wallet.Create("EUR", 50m);

        Assert.Throws<DomainValidationException>(() => wallet.SubtractFunds(0m));
    }

    [Fact]
    public void ForceSubtractFunds_can_result_in_negative_balance()
    {
        var wallet = Wallet.Create("EUR", 10m);

        wallet.ForceSubtractFunds(25m);

        Assert.Equal(-15m, wallet.Balance);
    }

    [Fact]
    public void ForceSubtractFunds_reduces_balance_when_sufficient()
    {
        var wallet = Wallet.Create("USD", 100m);

        wallet.ForceSubtractFunds(40m);

        Assert.Equal(60m, wallet.Balance);
    }

    [Fact]
    public void ForceSubtractFunds_with_non_positive_amount_throws_DomainValidationException()
    {
        var wallet = Wallet.Create("EUR", 5m);

        Assert.Throws<DomainValidationException>(() => wallet.ForceSubtractFunds(0m));
    }
}
