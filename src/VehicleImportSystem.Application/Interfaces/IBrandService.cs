using VehicleImportSystem.Application.DTOs;

namespace VehicleImportSystem.Application.Interfaces;

/// <summary>
/// Service for retrieving vehicle brand and model dictionary data.
/// Provides access to car brands and their associated models from the database.
/// </summary>
public interface IBrandService
{
    /// <summary>
    /// Retrieves all vehicle brands sorted alphabetically by name.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of all brands.</returns>
    Task<List<BrandDto>> GetAllBrandsAsync(CancellationToken ct);
}