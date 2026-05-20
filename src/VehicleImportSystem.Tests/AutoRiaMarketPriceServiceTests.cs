using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Enums;
using VehicleImportSystem.Infrastructure.Services;
using VehicleImportSystem.Tests.Helpers;

namespace VehicleImportSystem.Tests;

public class AutoRiaMarketPriceServiceTests
{
    [Fact]
    public async Task GetAveragePriceAsync_ValidResponse_ReturnsUsdAndEur()
    {
        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "interQuartileMean": 20000, "total": 5 }""", Encoding.UTF8, "application/json")
            }));
        using var client = new HttpClient(handler);
        var sut = CreateService(client, eurRate: 42m, usdRate: 40m);

        var result = await sut.GetAveragePriceAsync(1, 10, 2019, FuelType.Petrol, 2.0m);

        result.PriceUsd.Should().Be(20000m);
        result.PriceEur.Should().Be(19048m);
    }

    [Fact]
    public async Task GetAveragePriceAsync_NullPrice_ReturnsZeroes()
    {
        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "interQuartileMean": null, "total": 0 }""", Encoding.UTF8, "application/json")
            }));
        using var client = new HttpClient(handler);
        var sut = CreateService(client);

        var result = await sut.GetAveragePriceAsync(1, 10, 2018, FuelType.Diesel, 2.2m);

        result.PriceUsd.Should().Be(0m);
        result.PriceEur.Should().Be(0m);
    }

    [Fact]
    public async Task GetModelsFromApiAsync_ApiSuccess_ReturnsMappedModels()
    {
        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""[{ "name":"Model A", "value": 100 }, { "name":"Model B", "value": 101 }]""", Encoding.UTF8, "application/json")
            }));
        using var client = new HttpClient(handler);
        var sut = CreateService(client);

        var result = await sut.GetModelsFromApiAsync(5);

        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new { Id = 100, Name = "Model A", BrandId = 5 });
        result[1].Should().BeEquivalentTo(new { Id = 101, Name = "Model B", BrandId = 5 });
    }

    [Fact]
    public async Task GetModelsFromApiAsync_ApiFailure_ReturnsEmptyList()
    {
        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            throw new HttpRequestException("fail", null, HttpStatusCode.BadGateway));
        using var client = new HttpClient(handler);
        var sut = CreateService(client);

        var result = await sut.GetModelsFromApiAsync(5);

        result.Should().BeEmpty();
    }

    private static AutoRiaMarketPriceService CreateService(HttpClient client, decimal eurRate = 42m, decimal usdRate = 39m)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AutoRia:ApiKey"] = "test-key"
            })
            .Build();

        var currencyService = new Mock<ICurrencyService>();
        currencyService.Setup(x => x.GetEuroRateAsync()).ReturnsAsync(eurRate);
        currencyService.Setup(x => x.GetUsdRateAsync()).ReturnsAsync(usdRate);

        return new AutoRiaMarketPriceService(
            HttpClientFactoryMock.Create(client),
            new MemoryCache(new MemoryCacheOptions()),
            config,
            currencyService.Object,
            NullLogger<AutoRiaMarketPriceService>.Instance);
    }
}
