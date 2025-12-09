using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// A "Fake" service for testing. Always returns a fixed car price.
/// Used to simulate Auto.ria response without wasting API limits.
/// </summary>
public class MockMarketPriceService : IMarketPriceService
{
    public Task<decimal> GetAveragePriceAsync(int markId, int modelId, int year)
    {
        // Let's imagine that any car costs $15,000
        return Task.FromResult(15000m);
    }
}