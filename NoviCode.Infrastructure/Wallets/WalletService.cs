using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Application.Wallets.DTOs;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;

namespace NoviCode.Infrastructure.Wallets;

public sealed class WalletService : IWalletService
{
    private readonly AppDbContext _dbContext;
    private readonly IWalletBalanceAdjustmentStrategyResolver _strategyResolver;

    public WalletService(AppDbContext dbContext, IWalletBalanceAdjustmentStrategyResolver strategyResolver)
    {
        _dbContext = dbContext;
        _strategyResolver = strategyResolver;
    }

    public async Task<CreateWalletResponse> CreateAsync(CreateWalletRequest request, CancellationToken cancellationToken = default)
    {
        var wallet = Wallet.Create(request.Currency, request.InitialBalance);

        _dbContext.Wallets.Add(wallet);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateWalletResponse
        {
            Id = wallet.Id,
            Balance = wallet.Balance,
            Currency = wallet.Currency
        };
    }

    public async Task<GetWalletBalanceResponse> GetBalanceAsync(long walletId, string? currency, CancellationToken cancellationToken = default)
    {
        var wallet = await _dbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == walletId, cancellationToken);

        if (wallet is null)
            throw new NotFoundException($"Wallet {walletId} was not found.");

        var walletCurrency = wallet.Currency;

        if (string.IsNullOrWhiteSpace(currency))
        {
            return new GetWalletBalanceResponse
            {
                WalletId = wallet.Id,
                Balance = wallet.Balance,
                Currency = walletCurrency
            };
        }

        var targetCurrency = Wallet.NormalizeCurrencyCode(currency);

        if (targetCurrency == walletCurrency)
        {
            return new GetWalletBalanceResponse
            {
                WalletId = wallet.Id,
                Balance = wallet.Balance,
                Currency = walletCurrency
            };
        }

        var convertedBalance = await ConvertAsync(wallet.Balance,walletCurrency,targetCurrency,cancellationToken);

        return new GetWalletBalanceResponse
        {
            WalletId = wallet.Id,
            Balance = convertedBalance,
            Currency = targetCurrency
        };
    }

    public async Task<AdjustWalletBalanceResponse> AdjustBalanceAsync(long walletId,decimal amount,string currency,string strategy,CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new ValidationException("Amount must be positive.");

        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == walletId, cancellationToken);

        if (wallet is null)
            throw new NotFoundException($"Wallet {walletId} was not found.");

        var requestCurrency = Wallet.NormalizeCurrencyCode(currency);
        var walletCurrency = wallet.Currency.Trim();

        decimal amountInWalletCurrency;

        if (requestCurrency == walletCurrency)
        {
            amountInWalletCurrency = amount;
        }
        else
        {
            amountInWalletCurrency = await ConvertAsync(amount,requestCurrency,walletCurrency,cancellationToken);
        }

        var adjustmentStrategy = _strategyResolver.Resolve(strategy);
        adjustmentStrategy.Apply(wallet, amountInWalletCurrency);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdjustWalletBalanceResponse
        {
            WalletId = wallet.Id,
            Balance = wallet.Balance,
            Currency = walletCurrency,
        };
    }

    private async Task<decimal> ConvertAsync(decimal amount,string fromCurrency,string toCurrency,CancellationToken cancellationToken)
    {
        var rates = await LoadLatestEurRatesAsync(cancellationToken);

        var amountInEur = ToEurAmount(amount, fromCurrency, rates);

        decimal converted;

        if (toCurrency == "EUR")
        {
            converted = amountInEur;
        }
        else
        {
            var rate = GetEurToTargetRate(rates, toCurrency);
            converted = amountInEur * rate;
        }

        return Math.Round(converted, 2, MidpointRounding.ToEven);
    }

    private static decimal ToEurAmount(decimal amount, string fromCurrency, IReadOnlyList<ExchangeRate> rates)
    {
        if (fromCurrency == "EUR")
            return amount;

         return amount / GetEurToTargetRate(rates, fromCurrency);
    }

    private static decimal GetEurToTargetRate(IReadOnlyList<ExchangeRate> rates, string targetCode)
    {
        if (targetCode == "EUR")
            return 1m;

        var row = rates.FirstOrDefault(x => string.Equals(x.TargetCurrency.Trim(), targetCode, StringComparison.OrdinalIgnoreCase));

        if (row is null)
            throw new ValidationException($"Missing exchange rate for {targetCode}.");

        if (row.Rate <= 0)
            throw new ValidationException($"Invalid exchange rate for {targetCode}.");

        return row.Rate;
    }

    private async Task<IReadOnlyList<ExchangeRate>> LoadLatestEurRatesAsync(CancellationToken cancellationToken)
    {
        if (!await _dbContext.ExchangeRates.AnyAsync(cancellationToken))
            throw new ValidationException("No exchange rates are available.");

        var latestDate = await _dbContext.ExchangeRates.MaxAsync(x => x.RateDate, cancellationToken);

        return await _dbContext.ExchangeRates
            .AsNoTracking()
            .Where(x => x.RateDate == latestDate && x.BaseCurrency == "EUR")
            .ToListAsync(cancellationToken);
    }
}
