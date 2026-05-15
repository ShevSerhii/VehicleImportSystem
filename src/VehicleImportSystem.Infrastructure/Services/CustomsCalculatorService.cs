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

    public CustomsCalculatorService(
        ICurrencyService currencyService,
        IMarketPriceService marketPriceService,
        IOptions<CustomsSettings> settings,
        IAppDbContext context)
    {
        _currencyService = currencyService;
        _marketPriceService = marketPriceService;
        _settings = settings.Value;
        _context = context;
    }

    /// <inheritdoc />
    public async Task<CalculationResultDto> CalculateAsync(CalculationRequest request, string userDeviceId)
    {
        decimal euroRate = await _currencyService.GetEuroRateAsync();
        decimal usdRate = await _currencyService.GetUsdRateAsync();

        var priceDto = await _marketPriceService.GetAveragePriceAsync(
            request.MarkId, request.ModelId, request.Year, request.FuelType, request.EngineCapacity);

        decimal marketPriceUsd = priceDto.PriceUsd;

        decimal usdToEurRate = usdRate / euroRate;
        decimal marketPriceEur = marketPriceUsd * usdToEurRate;

        decimal duty = request.FuelType == FuelType.Electric
            ? 0
            : request.PriceInEur * _settings.ImportDutyRate;

        decimal excise = CalculateExcise(request);

        decimal vatBase = request.PriceInEur + duty + excise;
        decimal vat = CalculateVat(request, vatBase);

        decimal pensionBaseEur = request.PriceInEur;

        decimal pensionFund = request.FuelType == FuelType.Electric
            ? 0
            : CalculatePensionFund(pensionBaseEur, euroRate);

        decimal totalTaxes = duty + excise + vat + pensionFund;
        decimal turnkeyPrice = request.PriceInEur + totalTaxes;
        decimal profit = marketPriceEur - turnkeyPrice;

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
    /// VAT: 20% from 2026 for all vehicles. EV may reduce base via <see cref="CalculationRequest.EvVatExemptShare"/>.
    /// </summary>
    private decimal CalculateVat(CalculationRequest request, decimal vatBase)
    {
        decimal taxableShare = 1m;

        if (request.FuelType == FuelType.Electric && request.EvVatExemptShare > 0)
        {
            taxableShare = 1m - Math.Clamp(request.EvVatExemptShare, 0m, 1m);
        }

        return vatBase * taxableShare * _settings.VatRate;
    }

    /// <summary>
    /// Excise per пп. 215.3.5-1 ПКУ: ICE formula, gas/LPG as petrol, hybrid schemes, EV per kWh.
    /// </summary>
    private decimal CalculateExcise(CalculationRequest request)
    {
        int age = GetExciseAgeCoefficient(request.Year);

        if (request.FuelType == FuelType.Hybrid)
        {
            var hybridScheme = request.HybridExciseScheme ?? HybridExciseScheme.FixedRate;

            return hybridScheme == HybridExciseScheme.ByIceEngine
                ? CalculateIceExcise(
                    request.HybridIceFuelType ?? FuelType.Petrol,
                    request.EngineCapacity,
                    age)
                : _settings.HybridRate;
        }

        if (request.FuelType == FuelType.Electric)
        {
            return _settings.ElectricRate * request.EngineCapacity;
        }

        var iceFuelType = MapGasToPetrolExciseFuelType(request.FuelType);
        return CalculateIceExcise(iceFuelType, request.EngineCapacity, age);
    }

    /// <summary>
    /// LPG / gas-petrol vehicles: excise uses petrol rates (engine displacement in cm³).
    /// </summary>
    private static FuelType MapGasToPetrolExciseFuelType(FuelType fuelType) =>
        fuelType switch
        {
            FuelType.Gas or FuelType.GasPetrol => FuelType.Petrol,
            _ => fuelType
        };

    private int GetExciseAgeCoefficient(int manufactureYear)
    {
        int age = DateTime.UtcNow.Year - manufactureYear;

        if (age < 1) age = 1;
        if (age > _settings.MaxExciseAge) age = _settings.MaxExciseAge;

        return age;
    }

    private decimal CalculateIceExcise(FuelType fuelType, int capacity, int age)
    {
        decimal volumeCoeff = capacity / 1000m;

        return fuelType switch
        {
            FuelType.Petrol when capacity <= _settings.PetrolVolumeThreshold
                => _settings.PetrolRateSmall * volumeCoeff * age,

            FuelType.Petrol
                => _settings.PetrolRateLarge * volumeCoeff * age,

            FuelType.Diesel when capacity <= _settings.DieselVolumeThreshold
                => _settings.DieselRateSmall * volumeCoeff * age,

            FuelType.Diesel
                => _settings.DieselRateLarge * volumeCoeff * age,

            _ => 0
        };
    }

    /// <summary>
    /// Pension fund fee at first registration: 3% / 4% / 5% by value in UAH (subsistence minimum tiers).
    /// </summary>
    private decimal CalculatePensionFund(decimal valEur, decimal rate)
    {
        decimal valUah = valEur * rate;
        decimal tier1 = _settings.SubsistenceMinimum * _settings.PensionTier1Multiplier;
        decimal tier2 = _settings.SubsistenceMinimum * _settings.PensionTier2Multiplier;

        decimal percent = valUah <= tier1
            ? _settings.PensionRateLow
            : valUah <= tier2
                ? _settings.PensionRateMedium
                : _settings.PensionRateHigh;

        return (valUah * percent) / rate;
    }
}
