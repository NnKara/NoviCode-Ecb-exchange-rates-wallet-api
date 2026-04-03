

namespace NoviCode.Application.Wallets.DTOs
{
    public sealed class CreateWalletResponse
    {
        public long Id { get; init; }
        public decimal Balance { get; init; }
        public string Currency { get; init; } = default!;
    }
}
