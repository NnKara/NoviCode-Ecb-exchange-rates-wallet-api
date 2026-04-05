using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Wallets.ENUM;
using NoviCode.Domain.Entities;
using NoviCode.Domain.Exceptions;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.Wallets;
using Xunit;

namespace NoviCode.Infrastructure.Tests.Wallets;

public sealed class WalletServiceAdjustBalanceTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AdjustBalanceAsync_subtract_reduces_balance_in_wallet_currency()
    {
        await using var db = CreateInMemoryDbContext();
        var sut = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());
        var id = await CreateWalletAsync(db, "USD", 100m);

        var result = await sut.AdjustBalanceAsync(id,30m,"USD",WalletBalanceStrategyNames.SubtractFunds,CancellationToken.None);

        Assert.Equal(id, result.WalletId);
        Assert.Equal(70m, result.Balance);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task AdjustBalanceAsync_subtract_when_insufficient_throws_DomainValidationException()
    {
        await using var db = CreateInMemoryDbContext();
        var sut = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());
        var id = await CreateWalletAsync(db, "USD", 10m);

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            sut.AdjustBalanceAsync(id,50m,"USD",WalletBalanceStrategyNames.SubtractFunds,CancellationToken.None));
    }

    [Fact]
    public async Task AdjustBalanceAsync_force_subtract_allows_negative_balance()
    {
        await using var db = CreateInMemoryDbContext();
        var sut = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());
        var id = await CreateWalletAsync(db, "EUR", 10m);

        var result = await sut.AdjustBalanceAsync(id,40m,"EUR",WalletBalanceStrategyNames.ForceSubtractFunds,CancellationToken.None);

        Assert.Equal(-30m, result.Balance);
        Assert.Equal("EUR", result.Currency);

        var stored = await db.Wallets.FindAsync(id);
        Assert.NotNull(stored);
        Assert.Equal(-30m, stored!.Balance);
    }

    private static async Task<long> CreateWalletAsync(AppDbContext db, string currency, decimal balance)
    {
        var wallet = Wallet.Create(currency, balance);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet.Id;
    }
}
