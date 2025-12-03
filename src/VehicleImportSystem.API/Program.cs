using Microsoft.AspNetCore.Mvc;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Domain.Settings;
using VehicleImportSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CustomsSettings>(
    builder.Configuration.GetSection("CustomsSettings"));

builder.Services.AddScoped<ICustomsCalculatorService, CustomsCalculatorService>();

// We use these "fake" services to make the app runnable without real external APIs.
// In the future, we will swap them for RealCurrencyService and AutoRiaService.
builder.Services.AddScoped<ICurrencyService, MockCurrencyService>();
builder.Services.AddScoped<IMarketPriceService, MockMarketPriceService>();

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

app.Run();