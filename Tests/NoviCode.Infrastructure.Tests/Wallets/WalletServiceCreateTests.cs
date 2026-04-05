using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Wallets.DTOs;
using NoviCode.Domain.Exceptions;
using NoviCode.Infrastructure.Data;
using NoviCode.Infrastructure.Wallets;
using Xunit;

namespace NoviCode.Infrastructure.Tests.Wallets;

public sealed class WalletServiceCreateTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;


        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_creates_wallet_and_returns_response()
    {
        await using var db = CreateInMemoryDbContext();
        var walletService = new WalletService(db, new WalletBalanceAdjustmentStrategyResolver());

        var result = await walletService.CreateAsync(new CreateWalletRequest { Currency = "USD", InitialBalance = 50m });

        Assert.True(result.Id > 0);
        Assert.Equal(50m, result.Balance);
        Assert.Equal("USD", result.Currency);

        var stored = await db.Wallets.FindAsync(result.Id);
        Assert.NotNull(stored);
        Assert.Equal(50m, stored!.Balance);
        Assert.Equal("USD", stored.Currency);
    }
}
