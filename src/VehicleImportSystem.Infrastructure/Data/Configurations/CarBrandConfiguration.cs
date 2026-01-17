using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the CarBrand entity.
/// Defines table structure, constraints, and relationships.
/// </summary>
public class CarBrandConfiguration : IEntityTypeConfiguration<CarBrand>
{
    public void Configure(EntityTypeBuilder<CarBrand> builder)
    {
        // We do not generate IDs automatically because we use external IDs from Auto.ria
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        // One-to-Many relationship: A Brand has many Models
        builder.HasMany(b => b.Models)
            .WithOne(m => m.Brand)
            .HasForeignKey(b => b.BrandId);
    }
}