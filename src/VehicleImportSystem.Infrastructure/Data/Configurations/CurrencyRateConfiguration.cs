using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Currency Rates.
/// </summary>
public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(3)
            .IsFixedLength() // ISO codes are always 3 chars (USD, EUR)
            .IsRequired();

        // High precision is required for financial exchange rates, for ex."rate":42.2721
        builder.Property(e => e.Rate)
            .HasColumnType("decimal(18,4)");
    }
}