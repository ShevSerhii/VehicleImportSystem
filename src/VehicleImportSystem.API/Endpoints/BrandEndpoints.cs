using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.API.Endpoints;

/// <summary>
/// API endpoints for vehicle brands and models dictionary data.
/// Provides access to car brands and their associated models from the database.
/// </summary>
public static class BrandEndpoints
{
    /// <summary>
    /// Maps brand-related endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/brands").WithTags("Dictionary Data");

        group.MapGet("/", async ([FromServices] IBrandService service, CancellationToken ct)
            => Results.Ok(await service.GetAllBrandsAsync(ct)));

        group.MapGet("/{id}/models", async (int id, [FromServices] IBrandService service, CancellationToken ct)
            => Results.Ok(await service.GetModelsByBrandIdAsync(id, ct)));
    }
}