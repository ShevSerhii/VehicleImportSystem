using Microsoft.Extensions.Options;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Domain.Enums;
using VehicleImportSystem.Domain.Settings;

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

        decimal marketPriceUsd = await _marketPriceService.GetAveragePriceAsync(
            request.MarkId, request.ModelId, request.Year);

        // Temporary conversion (USD -> EUR).
        decimal marketPriceEur = marketPriceUsd * 0.95m; // TODO: Use real cross-rate later.

        // Electric vehicles pay 0% duty.
        decimal duty = (request.FuelType == FuelType.Electric)
            ? 0
            : request.PriceInEur * _settings.ImportDutyRate;

        decimal excise = CalculateExcise(request.FuelType, request.EngineCapacity, request.Year);

        // Base = Price + Duty + Excise. EV pays 0% VAT.
        decimal vatBase = request.PriceInEur + duty + excise;
        decimal vat = (request.FuelType == FuelType.Electric)
            ? 0
            : vatBase * _settings.VatRate;

        // Base includes VAT. Calculated in UAH based on thresholds.
        decimal pensionBaseEur = request.PriceInEur + duty + excise + vat;
        decimal pensionFund = CalculatePensionFund(pensionBaseEur, euroRate);

        decimal totalTaxes = duty + excise + vat + pensionFund;
        decimal turnkeyPrice = request.PriceInEur + totalTaxes;
        decimal profit = marketPriceEur - turnkeyPrice;

        var record = new CalculationRecord
        {
            UserDeviceId = userDeviceId,
            BrandId = request.MarkId,
            ModelId = request.ModelId,
            Year = request.Year,
            FuelType = request.FuelType,
            EngineCapacity = request.EngineCapacity,
            PriceInEur = request.PriceInEur,
            TotalTurnkeyPrice = turnkeyPrice,
            PotentialProfit = profit,
            CreatedAt = DateTime.UtcNow
        };

        _context.CalculationRecords.Add(record);
        await _context.SaveChangesAsync(default);

        return new CalculationResultDto
        {
            ImportDuty = duty,
            ExciseTax = excise,
            Vat = vat,
            PensionFund = pensionFund,
            TotalCustomsClearance = totalTaxes,
            TotalVehicleCost = turnkeyPrice,
            MarketPrice = marketPriceEur,
            PotentialProfit = profit,
            IsProfitable = profit > 0,
            CurrencyRateUsed = euroRate
        };
    }

    /// <summary>
    /// Helper method to calculate Excise Tax based on vehicle type and age.
    /// </summary>
    private decimal CalculateExcise(FuelType fuelType, int capacity, int year)
    {
        int currentYear = DateTime.UtcNow.Year;
        int age = currentYear - year - 1;

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