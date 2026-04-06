using Microsoft.EntityFrameworkCore;
using NoviCode.Application.Exceptions;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;

namespace NoviCode.Infrastructure.Repositories;

public sealed class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _db;

    public WalletRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Wallet?> GetByIdAsync(long id, bool track, CancellationToken cancellationToken = default)
    {
        if (!track)
        {
            return await _db.Wallets.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        return await _db.Wallets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Add(Wallet wallet)
    {
        _db.Wallets.Add(wallet);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Wallet was modified by another request. Please retry.",ex);
        }
    }
}
