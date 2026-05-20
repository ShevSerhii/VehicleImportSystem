using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VehicleImportSystem.API.Endpoints;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Application.Validators;
using VehicleImportSystem.Domain.Enums;

namespace VehicleImportSystem.Tests;

public class ApiEndpointsTests
{
    [Fact]
    public async Task CalculatorEndpoint_ValidRequest_ReturnsOk()
    {
        var calcService = new Mock<ICustomsCalculatorService>();
        calcService.Setup(x => x.CalculateAsync(It.IsAny<CalculationRequest>(), "device-1"))
            .ReturnsAsync(new CalculationResultDto { TotalCustomsClearance = 123m });

        await using var app = await BuildTestApp(services =>
        {
            services.AddSingleton(calcService.Object);
            services.AddSingleton<IValidator<CalculationRequest>, CalculationRequestValidator>();
            services.AddSingleton(Mock.Of<IHistoryService>());
        });

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Device-Id", "device-1");

        var request = new CalculationRequest
        {
            MarkId = 1,
            ModelId = 10,
            Year = DateTime.UtcNow.Year - 2,
            FuelType = FuelType.Petrol,
            EngineCapacity = 2000,
            PriceInEur = 10000m
        };

        var response = await client.PostAsJsonAsync("/api/calculator/calculate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CalculationResultDto>();
        body!.TotalCustomsClearance.Should().Be(123m);
    }

    [Fact]
    public async Task CalculatorEndpoint_InvalidRequest_ReturnsBadRequest()
    {
        await using var app = await BuildTestApp(services =>
        {
            services.AddSingleton(Mock.Of<ICustomsCalculatorService>());
            services.AddSingleton<IValidator<CalculationRequest>, CalculationRequestValidator>();
            services.AddSingleton(Mock.Of<IHistoryService>());
        });

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Device-Id", "device-1");

        var badRequest = new CalculationRequest
        {
            MarkId = 0,
            ModelId = 0,
            Year = 1900,
            FuelType = FuelType.Petrol,
            EngineCapacity = -1,
            PriceInEur = 0
        };

        var response = await client.PostAsJsonAsync("/api/calculator/calculate", badRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HistoryEndpoints_GetDeleteClear_WorkAsExpected()
    {
        var historyService = new Mock<IHistoryService>();
        historyService.Setup(x => x.GetUserHistoryAsync("device-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalculationRecordDto> { new() { Id = 1, Year = 2020 } });
        historyService.Setup(x => x.DeleteRecordAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        historyService.Setup(x => x.DeleteRecordAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await using var app = await BuildTestApp(services =>
        {
            services.AddSingleton(Mock.Of<ICustomsCalculatorService>());
            services.AddSingleton<IValidator<CalculationRequest>, CalculationRequestValidator>();
            services.AddSingleton(historyService.Object);
        });

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Device-Id", "device-1");

        var getResponse = await client.GetAsync("/api/history/");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteOk = await client.DeleteAsync("/api/history/1");
        deleteOk.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteMissing = await client.DeleteAsync("/api/history/999");
        deleteMissing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var clearResponse = await client.DeleteAsync("/api/history/clear");
        clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task BrandCurrencyAndMarketEndpoints_ReturnExpectedResponses()
    {
        var brandService = new Mock<IBrandService>();
        brandService.Setup(x => x.GetAllBrandsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new BrandDto(1, "Audi") });

        var currencyService = new Mock<ICurrencyService>();
        currencyService.Setup(x => x.GetEuroRateAsync()).ReturnsAsync(42m);
        currencyService.Setup(x => x.GetUsdRateAsync()).ReturnsAsync(39m);

        var marketService = new Mock<IMarketPriceService>();
        marketService.Setup(x => x.GetModelsFromApiAsync(1))
            .ReturnsAsync(new List<ModelDto> { new(10, "A4", 1) });
        marketService.Setup(x => x.GetAveragePriceAsync(1, 10, 2020, FuelType.Petrol, 2.0m))
            .ReturnsAsync(new AveragePriceDto { PriceUsd = 20000m, PriceEur = 18500m });

        await using var app = await BuildTestApp(services =>
        {
            services.AddSingleton(Mock.Of<ICustomsCalculatorService>());
            services.AddSingleton<IValidator<CalculationRequest>, CalculationRequestValidator>();
            services.AddSingleton(Mock.Of<IHistoryService>());
            services.AddSingleton(brandService.Object);
            services.AddSingleton(currencyService.Object);
            services.AddSingleton(marketService.Object);
        });

        var client = app.GetTestClient();

        (await client.GetAsync("/api/brands/")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/currency/rates")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/market/brands/1/models")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/market/average-price?MarkId=1&ModelId=10&Year=2020&FuelType=Petrol&EngineVolume=2.0"))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CurrencyEndpoint_OnServiceFailure_ReturnsProblem()
    {
        var currencyService = new Mock<ICurrencyService>();
        currencyService.Setup(x => x.GetEuroRateAsync()).ThrowsAsync(new InvalidOperationException("NBU down"));

        await using var app = await BuildTestApp(services =>
        {
            services.AddSingleton(Mock.Of<ICustomsCalculatorService>());
            services.AddSingleton<IValidator<CalculationRequest>, CalculationRequestValidator>();
            services.AddSingleton(Mock.Of<IHistoryService>());
            services.AddSingleton(Mock.Of<IBrandService>());
            services.AddSingleton(currencyService.Object);
            services.AddSingleton(Mock.Of<IMarketPriceService>());
        });

        var client = app.GetTestClient();
        var response = await client.GetAsync("/api/currency/rates");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    private static async Task<WebApplication> BuildTestApp(Action<IServiceCollection> configureServices)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        configureServices(builder.Services);

        var app = builder.Build();
        app.MapCalculatorEndpoints();
        app.MapHistoryEndpoints();
        app.MapBrandEndpoints();
        app.MapCurrencyEndpoints();
        app.MapMarketEndpoints();
        await app.StartAsync();
        return app;
    }
}
