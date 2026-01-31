using VehicleImportSystem.Domain.Enums;

namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Parameters for querying the average market price.
/// </summary>
public record AveragePriceRequestDto(
    int MarkId,
    int ModelId,
    int Year,
    FuelType FuelType,
    decimal EngineVolume
);