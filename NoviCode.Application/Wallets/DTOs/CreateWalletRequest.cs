

namespace NoviCode.Application.Wallets.DTOs
{
    public sealed class CreateWalletRequest
    {
        public string Currency { get; init; } = default!;
        public decimal InitialBalance { get; init; }
    }
}
