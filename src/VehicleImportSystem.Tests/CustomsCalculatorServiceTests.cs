using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Domain.Enums;
using VehicleImportSystem.Domain.Settings;
using VehicleImportSystem.Infrastructure.Services;

namespace VehicleImportSystem.Tests;

public class CustomsCalculatorServiceTests
{
    private const decimal EurRate = 42m;
    private const decimal UsdRate = 38m;

    [Fact]
    public async Task CalculateAsync_PetrolEngine_ReturnsCorrectTaxes()
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 20000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Petrol,
            year: DateTime.UtcNow.Year - 5,
            engineCapacity: 2000,
            priceInEur: 10000m);

        // Act
        var result = await service.CalculateAsync(request, "device-1");

        // Assert
        result.ImportDuty.Should().Be(1000m);
        result.ExciseTax.Should().Be(500m);
        result.Vat.Should().Be(2300m);
        result.PensionFund.Should().Be(300m);
        result.TotalCustomsClearance.Should().Be(4100m);
    }

    [Fact]
    public async Task CalculateAsync_AgeCoefficient_CappedAt15Years()
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 20000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Petrol,
            year: DateTime.UtcNow.Year - 20,
            engineCapacity: 2000,
            priceInEur: 10000m);

        // Act
        var result = await service.CalculateAsync(request, "device-2");

        // Assert
        result.ExciseTax.Should().Be(1500m);
    }

    [Fact]
    public async Task CalculateAsync_ElectricVehicle_ReturnsZeroDutyAndPension()
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 32000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Electric,
            year: DateTime.UtcNow.Year - 3,
            engineCapacity: 75,
            priceInEur: 30000m);
        request.EvVatExemptShare = 0.5m;

        // Act
        var result = await service.CalculateAsync(request, "device-3");

        // Assert
        result.ImportDuty.Should().Be(0m);
        result.PensionFund.Should().Be(0m);
        result.ExciseTax.Should().Be(75m);
        result.Vat.Should().Be(3007.5m);
    }

    [Fact]
    public async Task CalculateAsync_HybridFixedRate_Returns100EuroExcise()
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 25000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Hybrid,
            year: DateTime.UtcNow.Year - 6,
            engineCapacity: 2000,
            priceInEur: 12000m);
        request.HybridExciseScheme = HybridExciseScheme.FixedRate;

        // Act
        var result = await service.CalculateAsync(request, "device-4");

        // Assert
        result.ExciseTax.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateAsync_HybridByIceEngine_ReturnsIceExcise()
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 25000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Hybrid,
            year: DateTime.UtcNow.Year - 8,
            engineCapacity: 2000,
            priceInEur: 12000m);
        request.HybridExciseScheme = HybridExciseScheme.ByIceEngine;
        request.HybridIceFuelType = FuelType.Petrol;

        // Act
        var result = await service.CalculateAsync(request, "device-5");

        // Assert
        result.ExciseTax.Should().Be(800m);
    }

    [Fact]
    public async Task CalculateAsync_GasFuel_UsesPetrolExciseRates()
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 21000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Gas,
            year: DateTime.UtcNow.Year - 5,
            engineCapacity: 2000,
            priceInEur: 10000m);

        // Act
        var result = await service.CalculateAsync(request, "device-gas");

        // Assert
        result.ExciseTax.Should().Be(500m);
    }

    [Fact]
    public async Task CalculateAsync_HybridWithoutScheme_UsesFixedRateByDefault()
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 23000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Hybrid,
            year: DateTime.UtcNow.Year - 4,
            engineCapacity: 1800,
            priceInEur: 11000m);
        request.HybridExciseScheme = null;

        // Act
        var result = await service.CalculateAsync(request, "device-hybrid-default");

        // Assert
        result.ExciseTax.Should().Be(100m);
    }

    [Theory]
    [InlineData(10000, 0.03, 300)]
    [InlineData(15000, 0.04, 600)]
    [InlineData(30000, 0.05, 1500)]
    public async Task CalculateAsync_PensionFundTiers_CalculatedCorrectly(
        decimal priceInEur,
        decimal pensionRate,
        decimal expectedPension)
    {
        // Arrange
        using var dbContext = CreateDbContextWithBrandAndModel();
        var service = CreateService(dbContext, marketPriceUsd: 25000m);
        var request = CreateBaseRequest(
            fuelType: FuelType.Petrol,
            year: DateTime.UtcNow.Year - 2,
            engineCapacity: 2000,
            priceInEur: priceInEur);

        // Act
        var result = await service.CalculateAsync(request, "device-tier");

        // Assert
        result.PensionFund.Should().Be(expectedPension);
        (result.PensionFund / priceInEur).Should().Be(pensionRate);
    }

    private static CustomsCalculatorService CreateService(TestAppDbContext dbContext, decimal marketPriceUsd)
    {
        var currencyService = new Mock<ICurrencyService>();
        currencyService.Setup(x => x.GetEuroRateAsync()).ReturnsAsync(EurRate);
        currencyService.Setup(x => x.GetUsdRateAsync()).ReturnsAsync(UsdRate);

        var marketService = new Mock<IMarketPriceService>();
        marketService.Setup(x => x.GetAveragePriceAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<FuelType>(),
                It.IsAny<decimal>()))
            .ReturnsAsync(new AveragePriceDto
            {
                PriceUsd = marketPriceUsd,
                PriceEur = marketPriceUsd * (UsdRate / EurRate)
            });
        marketService.Setup(x => x.GetModelsFromApiAsync(It.IsAny<int>()))
            .ReturnsAsync([]);

        var options = Options.Create(new CustomsSettings
        {
            ImportDutyRate = 0.10m,
            VatRate = 0.20m,
            PetrolRateSmall = 50m,
            PetrolRateLarge = 100m,
            DieselRateSmall = 75m,
            DieselRateLarge = 150m,
            HybridRate = 100m,
            ElectricRate = 1m,
            MaxExciseAge = 15,
            SubsistenceMinimum = 3328m,
            PensionTier1Multiplier = 165,
            PensionTier2Multiplier = 290,
            PensionRateLow = 0.03m,
            PensionRateMedium = 0.04m,
            PensionRateHigh = 0.05m,
            PetrolVolumeThreshold = 3000,
            DieselVolumeThreshold = 3500
        });

        return new CustomsCalculatorService(currencyService.Object, marketService.Object, options, dbContext);
    }

    private static CalculationRequest CreateBaseRequest(FuelType fuelType, int year, int engineCapacity, decimal priceInEur)
    {
        return new CalculationRequest
        {
            MarkId = 1,
            ModelId = 10,
            Year = year,
            FuelType = fuelType,
            EngineCapacity = engineCapacity,
            PriceInEur = priceInEur
        };
    }

    private static TestAppDbContext CreateDbContextWithBrandAndModel()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new TestAppDbContext(options);
        context.CarBrands.Add(new CarBrand { Id = 1, Name = "Brand-1" });
        context.CarModels.Add(new CarModel { Id = 10, Name = "Model-10", BrandId = 1 });
        context.SaveChanges();
        return context;
    }

    private sealed class TestAppDbContext : DbContext, IAppDbContext
    {
        public TestAppDbContext(DbContextOptions<TestAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<CarBrand> CarBrands => Set<CarBrand>();
        public DbSet<CarModel> CarModels => Set<CarModel>();
        public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
        public DbSet<CustomsCalculation> CustomsCalculation => Set<CustomsCalculation>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
