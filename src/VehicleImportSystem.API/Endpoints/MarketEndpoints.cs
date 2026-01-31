using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Enums;

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
        group.MapGet("/brands/{markId}/models", async (int markId, [FromServices] IMarketPriceService service)
            => Results.Ok(await service.GetModelsFromApiAsync(markId)))
            .WithName("GetModelsByBrand")
            .WithOpenApi();

        /// <summary>
        /// Calculates the average market price for a specific vehicle configuration.
        /// Returns price in both USD (from market) and EUR (converted).
        /// </summary>
        group.MapGet("/average-price", async (
            [AsParameters] AveragePriceRequestDto request,
            [FromServices] IMarketPriceService service) =>
        {
            var result = await service.GetAveragePriceAsync(
                request.MarkId,
                request.ModelId,
                request.Year,
                request.FuelType,
                request.EngineVolume);

            return Results.Ok(result);
        })
        .WithName("GetAveragePrice")
        .WithOpenApi();
    }
}