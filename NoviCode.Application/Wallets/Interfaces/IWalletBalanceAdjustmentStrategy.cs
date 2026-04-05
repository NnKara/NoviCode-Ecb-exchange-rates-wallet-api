using NoviCode.Domain.Entities;


namespace NoviCode.Application.Wallets.Interfaces
{
    public interface IWalletBalanceAdjustmentStrategy
    {
        void Apply(Wallet wallet, decimal amountInWalletCurrency);
    }
}
