using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Settings;
using VehicleImportSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CustomsSettings>(
    builder.Configuration.GetSection("CustomsSettings"));

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
    // Simulate a User Device ID (in a real app, this comes from HTTP Headers)
    // For now, we generate a random ID for testing purposes.
    string userDeviceId = Guid.NewGuid().ToString();

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

app.Run();