using NoviCode.Application.Exceptions;
using NoviCode.Application.Wallets.WalletBalanceStrategyKinds;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Application.Wallets.WalletBalanceStrategies;


namespace NoviCode.Infrastructure.Wallets
{
    public sealed class WalletBalanceAdjustmentStrategyResolver : IWalletBalanceAdjustmentStrategyResolver
    {
        private static readonly AddFundsStrategy AddFunds = new();
        private static readonly SubtractFundsStrategy SubtractFunds = new();
        private static readonly ForceSubtractFundsStrategy ForceSubtractFunds = new();

        public IWalletBalanceAdjustmentStrategy Resolve(string strategyName)
        {
            if (string.IsNullOrWhiteSpace(strategyName))
                throw new ValidationException("Strategy is required.");

            var strategy = strategyName.Trim();

            if (string.Equals(strategy, WalletBalanceStrategyNames.AddFunds, StringComparison.OrdinalIgnoreCase))
                return AddFunds;

            if (string.Equals(strategy, WalletBalanceStrategyNames.SubtractFunds, StringComparison.OrdinalIgnoreCase))
                return SubtractFunds;

            if (string.Equals(strategy, WalletBalanceStrategyNames.ForceSubtractFunds, StringComparison.OrdinalIgnoreCase))
                return ForceSubtractFunds;

            throw new ValidationException($"Unsupported strategy '{strategyName}'.");
        }
    }
}
