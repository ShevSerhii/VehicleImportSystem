using FluentAssertions;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Mappings;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Domain.Enums;
using VehicleImportSystem.Infrastructure.DTOs;

namespace VehicleImportSystem.Tests;

public class MappingExtensionsTests
{
    [Fact]
    public void ToDto_CarBrand_MapsFields()
    {
        var entity = new CarBrand { Id = 10, Name = "Toyota" };

        var dto = entity.ToDto();

        dto.Id.Should().Be(10);
        dto.Name.Should().Be("Toyota");
    }

    [Fact]
    public void ToDto_CarModel_MapsFields()
    {
        var entity = new CarModel { Id = 20, Name = "Camry", BrandId = 10 };

        var dto = entity.ToDto();

        dto.Id.Should().Be(20);
        dto.Name.Should().Be("Camry");
        dto.BrandId.Should().Be(10);
    }

    [Fact]
    public void ToDto_CustomsCalculation_MapsFields()
    {
        var createdAt = DateTime.UtcNow;
        var entity = new CustomsCalculation
        {
            Id = 1,
            CreatedAt = createdAt,
            Brand = new CarBrand { Name = "BMW" },
            Model = new CarModel { Name = "X5" },
            Year = 2020,
            PriceInEur = 20000m,
            TotalTurnkeyPrice = 26000m,
            PotentialProfit = 1500m
        };

        var dto = entity.ToDto();

        dto.Id.Should().Be(1);
        dto.Date.Should().Be(createdAt);
        dto.BrandName.Should().Be("BMW");
        dto.ModelName.Should().Be("X5");
        dto.Year.Should().Be(2020);
        dto.PriceInEur.Should().Be(20000m);
        dto.TotalTurnkeyPrice.Should().Be(26000m);
        dto.PotentialProfit.Should().Be(1500m);
    }

    [Fact]
    public void ToEntity_AutoRiaItemDto_MapsFields()
    {
        var dto = new AutoRiaItemDto { Value = 77, Name = "Skoda" };

        var entity = dto.ToEntity();

        entity.Id.Should().Be(77);
        entity.Name.Should().Be("Skoda");
    }

    [Fact]
    public void ToEntity_ModelDto_MapsFields()
    {
        var dto = new ModelDto(31, "Octavia", 77);

        var entity = dto.ToEntity(77);

        entity.Id.Should().Be(31);
        entity.Name.Should().Be("Octavia");
        entity.BrandId.Should().Be(77);
    }

    [Fact]
    public void ToEntity_CurrencyRate_MapsFields()
    {
        var rate = 42.5m;

        var entity = rate.ToEntity("EUR");

        entity.CurrencyCode.Should().Be("EUR");
        entity.Rate.Should().Be(42.5m);
        entity.ExchangeDate.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public void ToEntity_CalculationRequest_MapsCalculationRecordFields()
    {
        var request = new CalculationRequest
        {
            Year = 2019,
            FuelType = FuelType.Diesel,
            EngineCapacity = 2200,
            PriceInEur = 15000m
        };

        var entity = request.ToEntity("device-1", 1, 10, 4000m, 19000m, 1200m, 20200m);

        entity.UserDeviceId.Should().Be("device-1");
        entity.BrandId.Should().Be(1);
        entity.ModelId.Should().Be(10);
        entity.Year.Should().Be(2019);
        entity.FuelType.Should().Be(FuelType.Diesel);
        entity.EngineCapacity.Should().Be(2200);
        entity.PriceInEur.Should().Be(15000m);
        entity.TotalCustomsCost.Should().Be(4000m);
        entity.TotalTurnkeyPrice.Should().Be(19000m);
        entity.PotentialProfit.Should().Be(1200m);
        entity.MarketPriceSnapshot.Should().Be(20200m);
    }

    [Fact]
    public void ToResultDto_MapsAllFields()
    {
        var request = new CalculationRequest();

        var result = request.ToResultDto(
            duty: 1000m,
            excise: 500m,
            vat: 2200m,
            pensionFund: 300m,
            totalTaxes: 4000m,
            turnkeyPrice: 14000m,
            marketPrice: 16000m,
            profit: 2000m,
            exchangeRate: 42m);

        result.ImportDuty.Should().Be(1000m);
        result.ExciseTax.Should().Be(500m);
        result.Vat.Should().Be(2200m);
        result.PensionFund.Should().Be(300m);
        result.TotalCustomsClearance.Should().Be(4000m);
        result.TotalVehicleCost.Should().Be(14000m);
        result.MarketPrice.Should().Be(16000m);
        result.PotentialProfit.Should().Be(2000m);
        result.IsProfitable.Should().BeTrue();
        result.CurrencyRateUsed.Should().Be(42m);
    }

    [Fact]
    public void ToResultDto_NegativeProfit_SetsIsProfitableFalse()
    {
        var request = new CalculationRequest();

        var result = request.ToResultDto(0m, 0m, 0m, 0m, 0m, 18000m, 15000m, -3000m, 42m);

        result.IsProfitable.Should().BeFalse();
    }
}
