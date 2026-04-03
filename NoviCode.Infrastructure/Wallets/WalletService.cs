using NoviCode.Application.Wallets.DTOs;
using NoviCode.Application.Wallets.Interfaces;
using NoviCode.Domain.Entities;
using NoviCode.Infrastructure.Data;

namespace NoviCode.Infrastructure.Wallets
{
    public sealed class WalletService : IWalletService
    {

        private readonly AppDbContext _dbContext;

        public WalletService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<CreateWalletResponse> CreateAsync(CreateWalletRequest request, CancellationToken cancellationToken = default)
        {
            var wallet = Wallet.Create(request.Currency, request.InitialBalance);

            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateWalletResponse
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                Currency = wallet.Currency
            };
        }
    }
}
