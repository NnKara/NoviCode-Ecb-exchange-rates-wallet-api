using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Domain.Entities;

namespace NoviCode.Application.Wallets.WalletBalanceStrategies
{
    public sealed class ForceSubtractFundsStrategy : IWalletBalanceAdjustmentStrategy
    {
        public void Apply(Wallet wallet, decimal amountInWalletCurrency)
        {
            wallet.ForceSubtractFunds(amountInWalletCurrency);
        }
    }
}
