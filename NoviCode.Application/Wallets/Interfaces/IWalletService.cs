using NoviCode.Application.Wallets.DTOs;

namespace NoviCode.Application.Wallets.Interfaces
{
    public interface IWalletService
    {
        Task<CreateWalletResponse> CreateAsync(CreateWalletRequest request, CancellationToken cancellationToken = default);

        Task<GetWalletBalanceResponse> GetBalanceAsync(long walletId, string? currency, CancellationToken cancellationToken = default);

        Task<AdjustWalletBalanceResponse> AdjustBalanceAsync(long walletId, decimal amount,string currency, string strategy, CancellationToken cancellationToken = default);
    }
}
