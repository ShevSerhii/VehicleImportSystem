using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Infrastructure.DTOs;

namespace VehicleImportSystem.Application.Mappings;

/// <summary>
/// Extension methods for mapping domain entities to DTOs.
/// Provides a centralized location for entity-to-DTO conversion logic.
/// </summary>
public static class MappingExtensions
{
    // ENTITY -> DTO (READ)

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
    public static CalculationRecordDto ToDto(this CustomsCalculation entity)
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

    /// <summary>
    /// Maps a raw Auto.Ria model response to the internal ModelDto.
    /// </summary>
    public static ModelDto ToDto(this AutoRiaItemDto apiResponse, int brandId)
    {
        return new ModelDto(apiResponse.Value, apiResponse.Name, brandId);
    }

    /// <summary>
    /// Maps a pair of values (EUR, USD) to the client-facing DTO.
    /// Uses C# Tuple: (decimal, decimal).
    /// </summary>
    public static CurrencyRatesDto ToDto(this (decimal Eur, decimal Usd) rates)
    {
        return new CurrencyRatesDto
        {
            Eur = rates.Eur,
            Usd = rates.Usd,
            Date = DateTime.UtcNow.ToString("O") // ISO 8601
        };
    }

    // DTO -> ENTITY / RESULT (WRITE & CALC)

    /// <summary>
    /// Converts the DTO from the brands.json file to the CarBrand entity.
    /// </summary>
    public static CarBrand ToEntity(this AutoRiaItemDto dto)
    {
        return new CarBrand
        {
            Id = dto.Value,
            Name = dto.Name
        };
    }

    /// <summary>
    /// Maps a model (ModelDto) to an entity (CarModel). 
    /// Used when saving a new model fetched from the API.
    /// </summary>
    public static CarModel ToEntity(this ModelDto dto, int brandId)
    {
        return new CarModel
        {
            Id = dto.Id,
            Name = dto.Name,
            BrandId = brandId
        };
    }

    /// <summary>
    /// Maps a currency rate value to a database entity.
    /// </summary>
    public static CurrencyRate ToEntity(this decimal rate, string currencyCode)
    {
        return new CurrencyRate
        {
            CurrencyCode = currencyCode,
            Rate = rate,
            ExchangeDate = DateTime.UtcNow.Date
        };
    }

    /// <summary>
    /// Maps the query and calculation results into a history record (CalculationRecord).
    /// </summary>
    public static CustomsCalculation ToEntity(
        this CalculationRequest request,
        string userDeviceId,
        int? brandId,
        int? modelId,
        decimal totalTaxes,
        decimal turnkeyPrice,
        decimal profit,
        decimal marketPriceSnapshot)
    {
        return new CustomsCalculation
        {
            UserDeviceId = userDeviceId,
            BrandId = brandId,
            ModelId = modelId,
            Year = request.Year,
            FuelType = request.FuelType,
            EngineCapacity = request.EngineCapacity,
            PriceInEur = request.PriceInEur,

            TotalCustomsCost = totalTaxes,
            TotalTurnkeyPrice = turnkeyPrice,
            MarketPriceSnapshot = marketPriceSnapshot,
            PotentialProfit = profit,

            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Assembles the calculation results into the final DTO for the client.
    /// </summary>
    public static CalculationResultDto ToResultDto(
        this CalculationRequest request,
        decimal duty,
        decimal excise,
        decimal vat,
        decimal pensionFund,
        decimal totalTaxes,
        decimal turnkeyPrice,
        decimal marketPrice,
        decimal profit,
        decimal exchangeRate)
    {
        return new CalculationResultDto
        {
            ImportDuty = duty,
            ExciseTax = excise,
            Vat = vat,
            PensionFund = pensionFund,
            TotalCustomsClearance = totalTaxes,
            TotalVehicleCost = turnkeyPrice,
            MarketPrice = marketPrice,
            PotentialProfit = profit,
            IsProfitable = profit > 0,
            CurrencyRateUsed = exchangeRate
        };
    }
}