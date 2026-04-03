using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoviCode.Domain.Entities;


namespace NoviCode.Infrastructure.Configurations
{
    public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Balance).HasPrecision(18,2).IsRequired();

            builder.Property(x => x.Currency).HasColumnType("char(3)").IsRequired();
        }
    }
}
