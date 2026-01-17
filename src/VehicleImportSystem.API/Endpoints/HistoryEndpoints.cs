using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.API.Endpoints;

/// <summary>
/// API endpoints for calculation history management.
/// Allows users to retrieve, delete individual records, or clear their entire history.
/// </summary>
public static class HistoryEndpoints
{
    /// <summary>
    /// Maps history-related endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/history").WithTags("History");

        group.MapGet("/", async ([FromHeader(Name = "X-Device-Id")] string userDeviceId, [FromServices] IHistoryService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(userDeviceId))
                return Results.BadRequest("Header 'X-Device-Id' is required.");

            return Results.Ok(await service.GetUserHistoryAsync(userDeviceId, ct));
        });

        group.MapDelete("/{id}", async (int id, [FromServices] IHistoryService service, CancellationToken ct) =>
        {
            return await service.DeleteRecordAsync(id, ct) ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/clear", async ([FromHeader(Name = "X-Device-Id")] string userDeviceId, [FromServices] IHistoryService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(userDeviceId))
                return Results.BadRequest("Header 'X-Device-Id' is required.");

            await service.ClearUserHistoryAsync(userDeviceId, ct);

            return Results.NoContent();
        });
    }
}