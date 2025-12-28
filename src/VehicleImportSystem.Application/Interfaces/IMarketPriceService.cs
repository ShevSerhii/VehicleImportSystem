using VehicleImportSystem.Application.DTOs;

namespace VehicleImportSystem.Application.Interfaces;

/// <summary>
/// Service responsible for retrieving real-time market analytics data from external APIs (e.g., Auto.ria).
/// </summary>
public interface IMarketPriceService
{
    /// <summary>
    /// Retrieves a list of models for a specific brand directly from the external API.
    /// </summary>
    /// <param name="markId">The brand ID from the external system.</param>
    /// <returns>List of models.</returns>
    Task<List<ModelDto>> GetModelsFromApiAsync(int markId);

    /// <summary>
    /// Gets the average market price for a vehicle based on its parameters.
    /// </summary>
    /// <param name="markId">Brand ID.</param>
    /// <param name="modelId">Model ID.</param>
    /// <param name="year">Manufacturing year.</param>
    /// <returns>Average price in USD.</returns>
    Task<decimal> GetAveragePriceAsync(int markId, int modelId, int year);
}