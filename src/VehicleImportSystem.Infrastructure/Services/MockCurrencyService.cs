using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// A "Fake" service for testing. Always returns a fixed Euro rate.
/// Used when NBU API is unavailable or during development.
/// </summary>
public class MockCurrencyService : ICurrencyService
{
    public Task<decimal> GetEuroRateAsync()
    {
        // We return the fixed rate UAH per Euro
        return Task.FromResult(49.20m);
    }
}