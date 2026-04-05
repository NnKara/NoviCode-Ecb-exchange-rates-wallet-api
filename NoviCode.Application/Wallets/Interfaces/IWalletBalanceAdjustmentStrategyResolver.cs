namespace NoviCode.Application.Wallets.Interfaces;

public interface IWalletBalanceAdjustmentStrategyResolver
{
    IWalletBalanceAdjustmentStrategy Resolve(string strategyName);
}
