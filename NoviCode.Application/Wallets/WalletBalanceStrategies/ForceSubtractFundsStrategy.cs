using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
