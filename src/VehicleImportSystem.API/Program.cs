using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.API.Endpoints;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Settings;
using VehicleImportSystem.Infrastructure;
using VehicleImportSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CustomsSettings>(
    builder.Configuration.GetSection("CustomsSettings"));

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/calculator/calculate", async (
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

// Test endpoint to verify currency service
app.MapGet("/api/currency/euro-rate", async (
    [FromServices] ICurrencyService currencyService) =>
{
    try
    {
        var rate = await currencyService.GetEuroRateAsync();
        return Results.Ok(new { EuroRate = rate, RetrievedAt = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to get EUR rate: {ex.Message}");
    }
})
.WithName("GetEuroRate")
.WithOpenApi();

app.MapBrandEndpoints();
app.MapHistoryEndpoints();

app.Run();