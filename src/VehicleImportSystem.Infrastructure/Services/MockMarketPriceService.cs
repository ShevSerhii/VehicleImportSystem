using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// Mock implementation of market price service for testing purposes.
/// Always returns a fixed car price without making external API calls.
/// Used to simulate Auto.ria response without wasting API limits during development.
/// </summary>
public class MockMarketPriceService : IMarketPriceService
{
    /// <summary>
    /// Returns a fixed average market price for any vehicle configuration.
    /// </summary>
    /// <param name="markId">The brand identifier (ignored in mock implementation).</param>
    /// <param name="modelId">The model identifier (ignored in mock implementation).</param>
    /// <param name="year">The vehicle year (ignored in mock implementation).</param>
    /// <returns>A fixed price of $15,000 USD.</returns>
    public Task<decimal> GetAveragePriceAsync(int markId, int modelId, int year)
    {
        // Let's imagine that any car costs $15,000
        return Task.FromResult(15000m);
    }

    /// <summary>
    /// Not implemented in mock service. Throws NotImplementedException.
    /// </summary>
    /// <param name="markId">The brand identifier.</param>
    /// <returns>Throws NotImplementedException.</returns>
    /// <exception cref="NotImplementedException">Always thrown, as this method is not implemented in the mock service.</exception>
    public Task<List<ModelDto>> GetModelsFromApiAsync(int markId)
    {
        throw new NotImplementedException();
    }
}