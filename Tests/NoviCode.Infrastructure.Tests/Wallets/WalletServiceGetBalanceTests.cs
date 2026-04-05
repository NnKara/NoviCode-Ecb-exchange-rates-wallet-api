using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.Wallets;
using Xunit;

namespace NoviCode.Infrastructure.Tests.Wallets;

public sealed class WalletServiceGetBalanceTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetBalanceAsync_wallet_missing_throws_NotFoundException()
    {
        await using var db = CreateInMemoryDbContext();
        var walletService = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            walletService.GetBalanceAsync(999L, null));
    }

    [Fact]
    public async Task GetBalanceAsync_without_currency_returns_stored_balance()
    {
        await using var db = CreateInMemoryDbContext();
        var walletService = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());
        var id = await CreateWalletAsync(db, "EUR", 125.50m);

        var result = await walletService.GetBalanceAsync(id, null);

        Assert.Equal(id, result.WalletId);
        Assert.Equal(125.50m, result.Balance);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public async Task GetBalanceAsync_same_currency_as_wallet_does_not_require_rates()
    {
        await using var db = CreateInMemoryDbContext();
        var walletService = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());
        var id = await CreateWalletAsync(db, "USD", 40m);

        var result = await walletService.GetBalanceAsync(id, "usd");

        Assert.Equal(40m, result.Balance);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task GetBalanceAsync_converts_using_latest_eur_based_rates()
    {
        await using var db = CreateInMemoryDbContext();
        var rateDate = new DateOnly(2024, 6, 10);

        SeedEurRatesForDate(db, rateDate, new Dictionary<string, decimal>
        {
            ["USD"] = 1.10m,
            ["GBP"] = 0.85m,
        });

        await db.SaveChangesAsync();

        var walletService = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());
        var id = await CreateWalletAsync(db, "USD", 100m);

        var result = await walletService.GetBalanceAsync(id, "GBP");

        Assert.Equal(id, result.WalletId);
        Assert.Equal("GBP", result.Currency);
        // 100 USD -> EUR: 100/1.10 : EUR -> GBP: * 0.85  =>  77.2727... -> 77.27
        Assert.Equal(77.27m, result.Balance);
    }

    [Fact]
    public async Task GetBalanceAsync_conversion_when_no_rates_throws_ValidationException()
    {
        await using var db = CreateInMemoryDbContext();
        var walletService = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());
        var id = await CreateWalletAsync(db, "USD", 10m);

        await Assert.ThrowsAsync<ValidationException>(() =>
            walletService.GetBalanceAsync(id, "GBP"));
    }

    private static async Task<long> CreateWalletAsync(AppDbContext db, string currency, decimal balance)
    {
        var wallet = Wallet.Create(currency, balance);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet.Id;
    }

    private static void SeedEurRatesForDate(AppDbContext db, DateOnly rateDate, IReadOnlyDictionary<string, decimal> eurToTargetRates)
    {
        var now = DateTime.UtcNow;
        foreach (var (target, rate) in eurToTargetRates)
        {
            db.ExchangeRates.Add(new ExchangeRate
            {
                RateDate = rateDate,
                BaseCurrency = "EUR",
                TargetCurrency = target,
                Rate = rate,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
    }
}
