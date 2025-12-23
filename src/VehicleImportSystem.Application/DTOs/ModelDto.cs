namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Data transfer object representing a vehicle model.
/// </summary>
/// <param name="Id">Unique identifier synchronized with Auto.ria system.</param>
/// <param name="Name">Model name (e.g., "Golf", "X5").</param>
/// <param name="BrandId">Foreign key to the parent brand.</param>
public record ModelDto(int Id, string Name, int BrandId);