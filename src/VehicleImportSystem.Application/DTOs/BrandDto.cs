namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Data transfer object representing a vehicle brand.
/// </summary>
/// <param name="Id">Unique identifier synchronized with Auto.ria system.</param>
/// <param name="Name">Official brand name (e.g., "Volkswagen", "BMW").</param>
public record BrandDto(int Id, string Name);