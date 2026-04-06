using NoviCode.Application.Exceptions;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Domain.Entities;

namespace NoviCode.Application.Wallets.WalletBalanceStrategies
{
    public sealed class SubtractFundsStrategy : IWalletBalanceAdjustmentStrategy
    {
        public void Apply(Wallet wallet, decimal amountInWalletCurrency)
        {
            if (wallet.Balance < amountInWalletCurrency)
                throw new InsufficientFundsException($"Insufficient funds. Available: {wallet.Balance} {wallet.Currency}, required: {amountInWalletCurrency}.");

            wallet.SubtractFunds(amountInWalletCurrency);
        }
    }
    
}
