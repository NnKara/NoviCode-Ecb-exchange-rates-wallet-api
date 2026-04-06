using NoviCode.Domain.Entities;

namespace NoviCode.Application.Wallets.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(long id, bool track, CancellationToken cancellationToken = default);
    void Add(Wallet wallet);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
