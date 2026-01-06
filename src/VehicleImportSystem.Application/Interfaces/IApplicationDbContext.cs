using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Application.Interfaces;

/// <summary>
/// Abstraction over the Database Context.
/// Allows the Application layer to access data without depending on Entity Framework implementation details directly.
/// </summary>
public interface IAppDbContext
{
    DbSet<CarBrand> CarBrands { get; }
    DbSet<CarModel> CarModels { get; }
    DbSet<CurrencyRate> CurrencyRates { get; }
    DbSet<CustomsCalculation> CalculationRecords { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}