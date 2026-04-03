using NoviCode.Application.Wallets.DTOs;

namespace NoviCode.Application.Wallets.Interfaces
{
    public interface IWalletService
    {
        Task<CreateWalletResponse> CreateAsync(CreateWalletRequest request, CancellationToken cancellationToken = default);
    }
}
