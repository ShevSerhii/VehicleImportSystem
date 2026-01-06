using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Application.Mappings;
using VehicleImportSystem.Infrastructure.DTOs;

namespace VehicleImportSystem.Infrastructure.Data;

/// <summary>
/// Responsible for setting up the database state on application startup.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Applies pending EF Core migrations and seeds the database with initial data (Car Brands) from a local JSON file.
    /// Implementation details:
    /// Migrates the database to the latest schema version.
    /// Checks if the 'CarBrands' table is empty. If not, skips the seeding process.
    /// Locates 'brands.json' in the build directory (or source directory during development).
    /// Deserializes the JSON and inserts brands with their original Auto.RIA IDs.
    /// </summary>
    /// <param name="context">The database context instance used for data access.</param>
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();


        var buildDir = AppContext.BaseDirectory;

        var filePath = Path.Combine(buildDir, "Data", "Seeds", "brands.json");

        if (!File.Exists(filePath))
        {
            var sourcePath = Path.GetFullPath(Path.Combine(buildDir, @"..\..\..\..\VehicleImportSystem.Infrastructure\Data\Seeds\brands.json"));
            if (File.Exists(sourcePath))
            {
                filePath = sourcePath;
            }
        }

        if (File.Exists(filePath))
        {
            var jsonString = await File.ReadAllTextAsync(filePath);
            var brandsDto = JsonSerializer.Deserialize<List<AutoRiaItemDto>>(jsonString);

            if (brandsDto != null && brandsDto.Any())
            {
                var existingBrandIds = await context.CarBrands
                    .Select(b => b.Id)
                    .ToListAsync();

                var newBrands = brandsDto
                    .Where(dto => !existingBrandIds.Contains(dto.Value))
                    .Select(dto => dto.ToEntity())
                    .ToList();

                if (newBrands.Any())
                {
                    await context.CarBrands.AddRangeAsync(newBrands);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}