using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Application.Wallets;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.ExchangeRates;
using NoviCode.Infrastructure.Repositories;
using NoviCode.Infrastructure.Wallets;
using Xunit;

namespace NoviCode.Infrastructure.Tests.Wallets;

public sealed class WalletServiceCurrencyConversionTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetBalanceAsync_when_missing_rate_throws_ValidationException()
    {
        await using var db = CreateInMemoryDbContext();
        SeedEurRate(db, new DateOnly(2024, 6, 10), "USD", 1.1m);
        await db.SaveChangesAsync();

        var walletService = CreateWalletService(db);
        var id = await CreateWalletAsync(db, "USD", 10m);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => walletService.GetBalanceAsync(id, "JPY"));
        Assert.Contains("Missing exchange rate", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetBalanceAsync_rounds_midpoint_to_even()
    {
        // Make conversion yield exactly 1.005 EUR before rounding:
        // wallet is USD 2.01, USD rate is 2 => 2.01 / 2 = 1.005 EUR -> rounds to 1.00 (ToEven)
        await using var db = CreateInMemoryDbContext();
        SeedEurRate(db, new DateOnly(2024, 6, 10), "USD", 2m);
        await db.SaveChangesAsync();

        var walletService = CreateWalletService(db);
        var walletId = await CreateWalletAsync(db, "USD", 2.01m);

        var result = await walletService.GetBalanceAsync(walletId, "EUR");

        Assert.Equal("EUR", result.Currency);
        Assert.Equal(1.00m, result.Balance);
    }

    private static WalletService CreateWalletService(AppDbContext db)
    {
        var walletRepository = new WalletRepository(db);
        var exchangeRatesReader = new LatestEurExchangeRatesReader(db);
        var strategyResolver = new WalletBalanceAdjustmentStrategyResolver();

        return new WalletService(walletRepository, exchangeRatesReader, strategyResolver);
    }

    private static async Task<long> CreateWalletAsync(AppDbContext db, string currency, decimal balance)
    {
        var wallet = Wallet.Create(currency, balance);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet.Id;
    }

    private static void SeedEurRate(AppDbContext db, DateOnly rateDate, string targetCurrency, decimal rate)
    {
        var now = DateTime.UtcNow;

        db.ExchangeRates.Add(new ExchangeRate
        {
            RateDate = rateDate,
            BaseCurrency = "EUR",
            TargetCurrency = targetCurrency,
            Rate = rate,
            CreatedAt = now,
            UpdatedAt = now
        });
    }
}

