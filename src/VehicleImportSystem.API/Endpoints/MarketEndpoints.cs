using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.API.Endpoints;

/// <summary>
/// API endpoints for interacting with external market data services.
/// Provides access to vehicle models and average market prices from Auto.ria API.
/// </summary>
public static class MarketEndpoints
{
    /// <summary>
    /// Maps market-related endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapMarketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/market").WithTags("Market Analytics");

        /// <summary>
        /// Retrieves available vehicle models for a specific brand from Auto.ria API.
        /// </summary>
        /// <param name="markId">The unique identifier of the vehicle brand (Auto.ria ID).</param>
        /// <param name="service">The market price service instance.</param>
        /// <returns>List of vehicle models for the specified brand.</returns>
        group.MapGet("/brands/{markId}/models", async (int markId, [FromServices] IMarketPriceService service)
            => Results.Ok(await service.GetModelsFromApiAsync(markId)));

        /// <summary>
        /// Calculates the average market price for a specific vehicle configuration.
        /// </summary>
        /// <param name="markId">The unique identifier of the vehicle brand.</param>
        /// <param name="modelId">The unique identifier of the vehicle model.</param>
        /// <param name="year">The manufacturing year of the vehicle.</param>
        /// <param name="service">The market price service instance.</param>
        /// <returns>Average market price in USD.</returns>
        group.MapGet("/average-price", async (
            [FromQuery] int markId,
            [FromQuery] int modelId,
            [FromQuery] int year,
            [FromServices] IMarketPriceService service)
            =>
        {
            var price = await service.GetAveragePriceAsync(markId, modelId, year);
            return Results.Ok(new { priceUSD = price });
        });
    }
}