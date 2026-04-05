using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoviCode.Application.Wallets.DTOs
{
    public sealed class AdjustWalletBalanceResponse
    {
        public long WalletId { get; init; }
        public decimal Balance { get; init; }
        public string Currency { get; init; } = default!;
    }
}
