using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the CarModel entity.
/// </summary>
public class CarModelConfiguration : IEntityTypeConfiguration<CarModel>
{
    public void Configure(EntityTypeBuilder<CarModel> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Name)
               .HasMaxLength(100)
               .IsRequired();
    }
}