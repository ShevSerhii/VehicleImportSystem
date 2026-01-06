using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Infrastructure.Data;

/// <summary>
/// The database context for the application.
/// </summary>
public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CarBrand> CarBrands { get; set; }
    public DbSet<CarModel> CarModels { get; set; }
    public DbSet<CurrencyRate> CurrencyRates { get; set; }
    public DbSet<CustomsCalculation> CalculationRecords { get; set; }

    /// <summary>
    /// Configures the model building process.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}