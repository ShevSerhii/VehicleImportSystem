using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the Calculation History table.
/// Includes indexes for performance optimization and decimal precision settings.
/// </summary>
public class CustomsCalculationConfiguration : IEntityTypeConfiguration<CustomsCalculation>
{
    public void Configure(EntityTypeBuilder<CustomsCalculation> builder)
    {
        builder.HasKey(e => e.Id);

        // Optimizes sorting history by date for a specific user
        builder.HasIndex(e => new { e.UserDeviceId, e.CreatedAt });

        builder.Property(e => e.UserDeviceId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(e => e.PriceInEur).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TotalCustomsCost).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TotalTurnkeyPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.MarketPriceSnapshot).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PotentialProfit).HasColumnType("decimal(18,2)");

        builder.HasOne(r => r.Brand)
                .WithMany()
                .HasForeignKey(r => r.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Model)
               .WithMany()
               .HasForeignKey(r => r.ModelId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}