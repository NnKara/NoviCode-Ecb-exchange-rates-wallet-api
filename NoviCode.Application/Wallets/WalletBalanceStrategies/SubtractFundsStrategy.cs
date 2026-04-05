using NoviCode.Application.Exceptions;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Domain.Entities;

namespace NoviCode.Application.Wallets.WalletBalanceStrategies
{
    public sealed class SubtractFundsStrategy : IWalletBalanceAdjustmentStrategy
    {
        public void Apply(Wallet wallet, decimal amountInWalletCurrency)
        {
            wallet.SubtractFunds(amountInWalletCurrency);
        }
    }
    
}
