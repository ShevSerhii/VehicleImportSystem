using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Tests.Helpers;

internal sealed class TestAppDbContext : DbContext, IAppDbContext
{
    public TestAppDbContext(DbContextOptions<TestAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<CarBrand> CarBrands => Set<CarBrand>();
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<CustomsCalculation> CustomsCalculation => Set<CustomsCalculation>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
