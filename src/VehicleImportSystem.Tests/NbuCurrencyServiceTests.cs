using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleImportSystem.Domain.Entities;
using VehicleImportSystem.Infrastructure.Services;
using VehicleImportSystem.Tests.Helpers;

namespace VehicleImportSystem.Tests;

public class NbuCurrencyServiceTests
{
    [Fact]
    public async Task GetEuroRateAsync_ApiSuccess_SavesAndReturnsRate()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""[{ "rate": 41.23 }]""", Encoding.UTF8, "application/json")
            }));
        using var client = new HttpClient(handler);
        var sut = CreateService(dbContext, client);

        var rate = await sut.GetEuroRateAsync();

        rate.Should().Be(41.23m);
        dbContext.CurrencyRates.Should().ContainSingle(x => x.CurrencyCode == "EUR" && x.Rate == 41.23m);
    }

    [Fact]
    public async Task GetEuroRateAsync_HttpFails_UsesLatestDbFallback()
    {
        await using var dbContext = CreateDbContext();
        dbContext.CurrencyRates.Add(new CurrencyRate
        {
            CurrencyCode = "EUR",
            Rate = 40.10m,
            ExchangeDate = DateTime.UtcNow.Date.AddDays(-1)
        });
        await dbContext.SaveChangesAsync();

        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            throw new HttpRequestException("boom", null, HttpStatusCode.InternalServerError));
        using var client = new HttpClient(handler);
        var sut = CreateService(dbContext, client);

        var rate = await sut.GetEuroRateAsync();

        rate.Should().Be(40.10m);
    }

    [Fact]
    public async Task GetEuroRateAsync_UsesTodayDbCacheWithoutHttpCall()
    {
        await using var dbContext = CreateDbContext();
        dbContext.CurrencyRates.Add(new CurrencyRate
        {
            CurrencyCode = "EUR",
            Rate = 42m,
            ExchangeDate = DateTime.UtcNow.Date
        });
        await dbContext.SaveChangesAsync();

        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""[{ "rate": 45.00 }]""", Encoding.UTF8, "application/json")
            }));
        using var client = new HttpClient(handler);
        var sut = CreateService(dbContext, client);

        var rate = await sut.GetEuroRateAsync();

        rate.Should().Be(42m);
        handler.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task GetEuroRateAsync_InvalidPayloadWithoutFallback_Throws()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DelegatingStubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""[{ "rate": 0 }]""", Encoding.UTF8, "application/json")
            }));
        using var client = new HttpClient(handler);
        var sut = CreateService(dbContext, client);

        var act = async () => await sut.GetEuroRateAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static TestAppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAppDbContext(options);
    }

    private static NbuCurrencyService CreateService(TestAppDbContext dbContext, HttpClient client)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NbuCurrency:EurUrl"] = "https://test.local/eur",
                ["NbuCurrency:UsdUrl"] = "https://test.local/usd"
            })
            .Build();

        return new NbuCurrencyService(
            HttpClientFactoryMock.Create(client),
            dbContext,
            NullLogger<NbuCurrencyService>.Instance,
            config,
            new MemoryCache(new MemoryCacheOptions()));
    }
}
