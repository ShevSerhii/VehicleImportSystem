using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Entities;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// Retrieves EUR rate from NBU public API with in-memory cache, DB cache, and fallback.
/// </summary>
public class NbuCurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<NbuCurrencyService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _eurEndpoint;
    private readonly string _usdEndpoint;

    /// <summary>
    /// Initializes a new instance of the NBU currency service.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="dbContext">Database context for caching rates.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="configuration">Configuration to read endpoints.</param>
    /// <param name="cache">Memory cache for temporary rate storage.</param>
    public NbuCurrencyService(
        IHttpClientFactory httpClientFactory,
        IAppDbContext dbContext,
        ILogger<NbuCurrencyService> logger,
        IConfiguration configuration,
        IMemoryCache cache)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
        _eurEndpoint = configuration.GetValue<string>("NbuCurrency:EurUrl")
                    ?? "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?valcode=EUR&json";
        _usdEndpoint = configuration.GetValue<string>("NbuCurrency:UsdUrl")
                    ?? "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?valcode=USD&json";
    }

    /// <summary>
    /// Returns today's EUR rate using memory cache, DB cache, or NBU API (with DB persistence).
    /// </summary>
    /// <returns>EUR exchange rate relative to UAH.</returns>
    public async Task<decimal> GetEuroRateAsync()
    {
        return await GetRateAsync("EUR", _eurEndpoint);
    }

    /// <summary>
    /// Returns today's USD rate using memory cache, DB cache, or NBU API (with DB persistence).
    /// </summary>
    /// <returns>USD exchange rate relative to UAH.</returns>
    public async Task<decimal> GetUsdRateAsync()
    {
        return await GetRateAsync("USD", _usdEndpoint);
    }

    /// <summary>
    /// Internal method to retrieve currency rate with caching and fallback logic.
    /// </summary>
    /// <param name="currencyCode">Currency code (EUR or USD).</param>
    /// <param name="endpoint">NBU API endpoint for the currency.</param>
    /// <returns>Exchange rate relative to UAH.</returns>
    private async Task<decimal> GetRateAsync(string currencyCode, string endpoint)
    {
        var today = DateTime.UtcNow.Date;
        var cacheKey = $"{currencyCode.ToLower()}-rate-{today:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out decimal cachedRate) && cachedRate > 0)
            return cachedRate;

        var cachedToday = await _dbContext.CurrencyRates
            .AsNoTracking()
            .Where(r => r.CurrencyCode == currencyCode && r.ExchangeDate.Date == today)
            .OrderByDescending(r => r.ExchangeDate)
            .FirstOrDefaultAsync();

        if (cachedToday is not null)
        {
            SetCache(cacheKey, cachedToday.Rate, today);
            return cachedToday.Rate;
        }

        try
        {
            var rates = await _httpClient.GetFromJsonAsync<List<NbuRateDto>>(endpoint);
            var rate = rates?.FirstOrDefault()?.Rate;

            if (rate is null || rate <= 0)
                throw new InvalidOperationException($"NBU response missing {currencyCode} rate.");

            var entity = new CurrencyRate
            {
                CurrencyCode = currencyCode,
                Rate = rate.Value,
                ExchangeDate = today
            };

            _dbContext.CurrencyRates.Add(entity);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            SetCache(cacheKey, rate.Value, today);
            return rate.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch {Currency} rate from NBU. Using last stored rate if available.", currencyCode);

            var fallback = await _dbContext.CurrencyRates
                .AsNoTracking()
                .Where(r => r.CurrencyCode == currencyCode)
                .OrderByDescending(r => r.ExchangeDate)
                .FirstOrDefaultAsync();

            if (fallback is not null)
            {
                SetCache(cacheKey, fallback.Rate, fallback.ExchangeDate.Date);
                return fallback.Rate;
            }

            throw;
        }
    }

    private void SetCache(string key, decimal rate, DateTime date)
    {
        var ttl = date.AddDays(1) - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
            ttl = TimeSpan.FromHours(1);

        _cache.Set(key, rate, ttl);
    }

    private sealed class NbuRateDto
    {
        public decimal Rate { get; set; }
    }
}