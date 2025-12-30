using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.API.Endpoints;

/// <summary>
/// API endpoints for currency exchange rate operations.
/// Provides access to current EUR and USD exchange rates relative to UAH.
/// </summary>
public static class CurrencyEndpoints
{
    /// <summary>
    /// Maps currency-related endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/currency").WithTags("Currency");

        /// <summary>
        /// Currency rates endpoint - returns both EUR and USD exchange rates.
        /// Used by the frontend to display current currency rates in the header.
        /// </summary>
        /// <param name="currencyService">The currency service instance.</param>
        /// <returns>Current EUR and USD exchange rates with timestamp.</returns>
        group.MapGet("/rates", async (
            [FromServices] ICurrencyService currencyService) =>
        {
            try
            {
                var eurRate = await currencyService.GetEuroRateAsync();
                var usdRate = await currencyService.GetUsdRateAsync();
                return Results.Ok(new 
                { 
                    Eur = eurRate, 
                    Usd = usdRate, 
                    Date = DateTime.UtcNow.ToString("O") // ISO 8601 format
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to get currency rates: {ex.Message}");
            }
        })
        .WithName("GetCurrencyRates")
        .WithOpenApi();
    }
}

