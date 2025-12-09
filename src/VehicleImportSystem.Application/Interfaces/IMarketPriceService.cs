namespace VehicleImportSystem.Application.Interfaces;

/// <summary>
/// Service responsible for retrieving market analytics data.
/// Usually integrates with external APIs like Auto.ria.
/// </summary>
public interface IMarketPriceService
{
    /// <summary>
    /// Gets the average market price for a vehicle based on its parameters.
    /// </summary>
    /// <param name="markId">Brand ID (from Auto.ria)</param>
    /// <param name="modelId">Model ID (from Auto.ria)</param>
    /// <param name="year">Manufacturing year</param>
    /// <returns>Average price (usually in USD, needs conversion logic if EUR is base)</returns>
    Task<decimal> GetAveragePriceAsync(int markId, int modelId, int year);
}