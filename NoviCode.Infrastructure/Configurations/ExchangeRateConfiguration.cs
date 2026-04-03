using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoviCode.Domain.Entities;

namespace NoviCode.Infrastructure.Configurations
{
    public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
    {
        public void Configure(EntityTypeBuilder<ExchangeRate> builder)
        {
            builder.ToTable("ExchangeRates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BaseCurrency).HasColumnType("char(3)").IsRequired();

            builder.Property(x => x.TargetCurrency).HasColumnType("char(3)").IsRequired();

            builder.Property(x => x.Rate).HasColumnType("decimal(18,8)").IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.UpdatedAt).IsRequired();

            builder.Property(x => x.RateDate).IsRequired();

            builder.HasIndex(x => new { x.RateDate, x.BaseCurrency, x.TargetCurrency}).IsUnique();
        }
    }
}
