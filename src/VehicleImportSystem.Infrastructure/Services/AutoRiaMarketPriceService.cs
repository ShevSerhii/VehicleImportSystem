using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Enums;
using VehicleImportSystem.Infrastructure.DTOs;
using VehicleImportSystem.Application.Mappings;
using VehicleImportSystem.Infrastructure.Services.AutoRia;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// Implementation of IMarketPriceService using Auto.ria API.
/// Includes caching, error handling for rate limits (429), and logging.
/// </summary>
public class AutoRiaMarketPriceService : IMarketPriceService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AutoRiaMarketPriceService> _logger;
    private readonly string _apiKey;

    // Auto.ria category ID for passenger cars
    private const int PassengerCarCategoryId = 1;

    public AutoRiaMarketPriceService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<AutoRiaMarketPriceService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("AutoRiaApi");
        _cache = cache;
        _logger = logger;
        _apiKey = configuration["AutoRia:ApiKey"]
                  ?? throw new ArgumentNullException("AutoRia API Key is missing in configuration");

        // Ensure User-Agent is set to prevent 403 Forbidden errors
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "VehicleImportSystem/1.0 (Dev)");
        }
    }

    /// <summary>
    /// Retrieves a list of vehicle models for a specific brand (Mark).
    /// Results are cached for 24 hours since model lists rarely change.
    /// </summary>
    /// <param name="markId">The Auto.RIA ID of the brand.</param>
    public async Task<List<ModelDto>> GetModelsFromApiAsync(int markId)
    {
        string cacheKey = $"autoriamodels_{markId}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Cache models for 1 day
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

            _logger.LogInformation("Cache miss. Fetching models for mark {MarkId} from API...", markId);

            var url = $"https://developers.ria.com/auto/categories/{PassengerCarCategoryId}/marks/{markId}/models?api_key={_apiKey}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<AutoRiaItemDto>>(url);

                return response?
                        .Select(x => x.ToDto(markId))
                        .ToList()
                        ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch models for mark {MarkId}", markId);

                // Do not cache failure for long (retry in 1 minute)
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return [];
            }
        }) ?? [];
    }

    /// <summary>
    /// Retrieves the average market price for a specific car model, year and fuel type.
    /// Uses InterQuartileMean to filter out outliers.
    /// </summary>
    public async Task<decimal> GetAveragePriceAsync(int markId, int modelId, int year, FuelType fuelType)
    {
        string cacheKey = $"avg_price_{markId}_{modelId}_{year}_{fuelType}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Market prices are stable, cache for 24 hours
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);

            int fuelId = (int)fuelType;

            _logger.LogInformation(
                "Fetching average price -> Mark: {Mark}, Model: {Model}, Year: {Year}, Fuel: {Fuel}...",
                markId, modelId, year, fuelType.ToString() ?? "All");

            // Build the base URL
            var url = $"https://developers.ria.com/auto/average_price?api_key={_apiKey}&category_id={PassengerCarCategoryId}&marka_id={markId}&model_id={modelId}&yers={year}&fuel_id={fuelId}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<AutoRiaAveragePriceResponse>(url);

                if (response == null)
                {
                    _logger.LogWarning("API returned null response for price query.");
                    return 0m;
                }

                _logger.LogInformation("Price received: {Price}", response.InterQuartileMean);
                return (decimal)response.InterQuartileMean;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Auto.RIA Rate Limit exceeded (429). Returning 0 and cooling down cache for 5 min.");

                // If rate limited, cache the "0" result only for 5 minutes, then try again
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching average price. URL: {Url}", url);
                return 0m;
            }
        });
    }
}