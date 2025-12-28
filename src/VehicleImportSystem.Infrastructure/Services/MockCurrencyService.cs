using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// A "Fake" service for testing. Always returns fixed currency rates.
/// Used when NBU API is unavailable or during development.
/// </summary>
public class MockCurrencyService : ICurrencyService
{
    /// <summary>
    /// Returns a fixed EUR rate for testing purposes.
    /// </summary>
    /// <returns>Fixed EUR exchange rate relative to UAH.</returns>
    public Task<decimal> GetEuroRateAsync()
    {
        // We return the fixed rate UAH per Euro
        return Task.FromResult(49.20m);
    }

    /// <summary>
    /// Returns a fixed USD rate for testing purposes.
    /// </summary>
    /// <returns>Fixed USD exchange rate relative to UAH.</returns>
    public Task<decimal> GetUsdRateAsync()
    {
        // We return the fixed rate UAH per USD
        return Task.FromResult(41.50m);
    }
}