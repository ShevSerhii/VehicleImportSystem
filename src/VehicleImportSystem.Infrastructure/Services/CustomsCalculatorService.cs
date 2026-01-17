using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Domain.Enums;
using VehicleImportSystem.Domain.Settings;
using VehicleImportSystem.Application.Mappings;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// Core implementation of the customs calculation logic.
/// Uses the Options Pattern to retrieve tax rates from configuration.
/// </summary>
public class CustomsCalculatorService : ICustomsCalculatorService
{
    private readonly ICurrencyService _currencyService;
    private readonly IMarketPriceService _marketPriceService;
    private readonly CustomsSettings _settings;
    private readonly IAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the service with required dependencies.
    /// </summary>
    /// <param name="currencyService">Service to get exchange rates.</param>
    /// <param name="marketPriceService">Service to get market analytics.</param>
    /// <param name="settings">Configuration options loaded from appsettings.json.</param>
    public CustomsCalculatorService(
        ICurrencyService currencyService,
        IMarketPriceService marketPriceService,
        IOptions<CustomsSettings> settings,
        IAppDbContext context)
    {
        _currencyService = currencyService;
        _marketPriceService = marketPriceService;
        _settings = settings.Value; // Extracting the actual settings object
        _context = context;
    }

    /// <inheritdoc />
    public async Task<CalculationResultDto> CalculateAsync(CalculationRequest request, string userDeviceId)
    {
        decimal euroRate = await _currencyService.GetEuroRateAsync();
        decimal usdRate = await _currencyService.GetUsdRateAsync();

        decimal marketPriceUsd = await _marketPriceService.GetAveragePriceAsync(
            request.MarkId, request.ModelId, request.Year, request.FuelType);

        // Convert USD to EUR using cross-rate: EUR/USD = (EUR/UAH) / (USD/UAH)
        decimal usdToEurRate = usdRate / euroRate;
        decimal marketPriceEur = marketPriceUsd * usdToEurRate;

        decimal duty = (request.FuelType == FuelType.Electric)
            ? 0
            : request.PriceInEur * _settings.ImportDutyRate;

        decimal excise = CalculateExcise(request.FuelType, request.EngineCapacity, request.Year);

        // EV pays 20% VAT in 2026
        decimal vatBase = request.PriceInEur + duty + excise;
        decimal vat =  vatBase * _settings.VatRate;

        // Base includes VAT. Calculated in UAH based on thresholds.
        decimal pensionBaseEur = request.PriceInEur;

        // Electric vehicles are EXEMPT from Pension Fund fee
        decimal pensionFund = (request.FuelType == FuelType.Electric)
           ? 0
            : CalculatePensionFund(pensionBaseEur, euroRate);

        decimal totalTaxes = duty + excise + vat + pensionFund;
        decimal turnkeyPrice = request.PriceInEur + totalTaxes;
        decimal profit = marketPriceEur - turnkeyPrice;

        // Validate that Brand and Model exist in database before saving
        // If they don't exist, set to null to avoid foreign key constraint violations
        int? validBrandId = null;
        if (request.MarkId > 0)
        {
            var brandExists = await _context.CarBrands.AnyAsync(b => b.Id == request.MarkId);
            if (brandExists)
            {
                validBrandId = request.MarkId;
            }
        }

        int? validModelId = null;
        if (request.ModelId > 0 && validBrandId.HasValue)
        {
            var existingModel = await _context.CarModels
                .FirstOrDefaultAsync(m => m.Id == request.ModelId);

            if (existingModel != null)
            {
                validModelId = existingModel.Id;
            }
            else
            {
                // Model not found in DB. Fetch details from API to get the Name.
                var modelsFromApi = await _marketPriceService.GetModelsFromApiAsync(request.MarkId);
                var targetModelDto = modelsFromApi.FirstOrDefault(x => x.Id == request.ModelId);

                if (targetModelDto != null)
                {
                    var newModel = targetModelDto.ToEntity(request.MarkId);

                    _context.CarModels.Add(newModel);

                    validModelId = newModel.Id;
                }
            }
        }

        var record = request.ToEntity(
            userDeviceId,
            validBrandId,
            validModelId,
            totalTaxes,
            turnkeyPrice,
            profit,
            marketPriceEur
        );

        _context.CustomsCalculation.Add(record);
        await _context.SaveChangesAsync(default);

        return request.ToResultDto(
            duty,
            excise,
            vat,
            pensionFund,
            totalTaxes,
            turnkeyPrice,
            marketPriceEur,
            profit,
            euroRate
        );
    }

    /// <summary>
    /// Helper method to calculate Excise Tax based on vehicle type and age.
    /// </summary>
    private decimal CalculateExcise(FuelType fuelType, int capacity, int year)
    {
        int currentYear = DateTime.UtcNow.Year;
        int age = currentYear - year;

        if (age < 1) age = 1;
        if (age > _settings.MaxExciseAge) age = _settings.MaxExciseAge;

        decimal volumeCoeff = capacity / 1000m;

        return fuelType switch
        {
            // PETROL: Check against PetrolVolumeThreshold (3000)
            FuelType.Petrol when capacity <= _settings.PetrolVolumeThreshold
                => _settings.PetrolRateSmall * volumeCoeff * age,

            FuelType.Petrol
                => _settings.PetrolRateLarge * volumeCoeff * age,

            // DIESEL: Check against DieselVolumeThreshold (3500)
            FuelType.Diesel when capacity <= _settings.DieselVolumeThreshold
                => _settings.DieselRateSmall * volumeCoeff * age,

            FuelType.Diesel
                => _settings.DieselRateLarge * volumeCoeff * age,

            // OTHER TYPES
            FuelType.Electric => _settings.ElectricRate * capacity,
            FuelType.Hybrid => _settings.HybridRate,

            _ => 0
        };
    }

    /// <summary>
    /// Helper method to calculate Pension Fund fee based on UAH value thresholds.
    /// </summary>
    private decimal CalculatePensionFund(decimal valEur, decimal rate)
    {
        decimal valUah = valEur * rate;
        decimal percent;

        if (valUah <= _settings.PensionThresholdTier1)
            percent = _settings.PensionRateLow;
        else if (valUah <= _settings.PensionThresholdTier2)
            percent = _settings.PensionRateMedium;
        else
            percent = _settings.PensionRateHigh;

        // Convert fee back to EUR
        return (valUah * percent) / rate;
    }
}