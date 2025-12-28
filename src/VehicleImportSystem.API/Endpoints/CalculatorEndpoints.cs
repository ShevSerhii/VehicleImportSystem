using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.API.Endpoints;

/// <summary>
/// API endpoints for customs calculation operations.
/// Provides functionality to calculate import duties, taxes, and analyze profitability.
/// </summary>
public static class CalculatorEndpoints
{
    /// <summary>
    /// Maps calculator-related endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapCalculatorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/calculator").WithTags("Calculator");

        /// <summary>
        /// Main calculation endpoint for customs duties and taxes.
        /// Calculates import duty, excise tax, VAT, pension fund, and total vehicle cost.
        /// Also compares with market price to determine profitability.
        /// </summary>
        /// <param name="request">Calculation input parameters (vehicle details, price, etc.)</param>
        /// <param name="calculatorService">The customs calculator service instance.</param>
        /// <returns>Detailed calculation result with tax breakdown and profitability analysis.</returns>
        group.MapPost("/calculate", async (
            [FromBody] CalculationRequest request,
            [FromServices] ICustomsCalculatorService calculatorService) =>
        {
            // Use UserDeviceId from request if provided, otherwise generate a new one for testing
            string userDeviceId = string.IsNullOrWhiteSpace(request.UserDeviceId)
                ? Guid.NewGuid().ToString()
                : request.UserDeviceId;

            var result = await calculatorService.CalculateAsync(request, userDeviceId);

            return Results.Ok(result);
        })
        .WithName("CalculateCustoms")
        .WithOpenApi();
    }
}

