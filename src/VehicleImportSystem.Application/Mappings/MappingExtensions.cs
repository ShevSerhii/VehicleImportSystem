using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Application.Mappings;

/// <summary>
/// Extension methods for mapping domain entities to DTOs.
/// Provides a centralized location for entity-to-DTO conversion logic.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps a CarBrand entity to a BrandDto.
    /// </summary>
    /// <param name="entity">The brand entity to map.</param>
    /// <returns>A BrandDto containing the brand's ID and name.</returns>
    public static BrandDto ToDto(this CarBrand entity)
        => new BrandDto(entity.Id, entity.Name);

    /// <summary>
    /// Maps a CarModel entity to a ModelDto.
    /// </summary>
    /// <param name="entity">The model entity to map.</param>
    /// <returns>A ModelDto containing the model's ID, name, and brand ID.</returns>
    public static ModelDto ToDto(this CarModel entity)
        => new ModelDto(entity.Id, entity.Name, entity.BrandId);

    /// <summary>
    /// Maps a CalculationRecord entity to a CalculationRecordDto.
    /// Includes brand and model names from navigation properties.
    /// </summary>
    /// <param name="entity">The calculation record entity to map.</param>
    /// <returns>A CalculationRecordDto with essential calculation information.</returns>
    public static CalculationRecordDto ToDto(this CalculationRecord entity)
    {
        return new CalculationRecordDto
        {
            Id = entity.Id,
            Date = entity.CreatedAt,
            BrandName = entity.Brand?.Name,
            ModelName = entity.Model?.Name,
            Year = entity.Year,
            PriceInEur = entity.PriceInEur,
            TotalTurnkeyPrice = entity.TotalTurnkeyPrice,
            PotentialProfit = entity.PotentialProfit
        };
    }
}