namespace VehicleImportSystem.Application.Interfaces;

/// <summary>
/// Service responsible for currency exchange operations.
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Retrieves the current EUR exchange rate relative to UAH.
    /// Should handle caching internally to minimize external API calls.
    /// </summary>
    /// <returns>Exchange rate</returns>
    Task<decimal> GetEuroRateAsync();

    /// <summary>
    /// Retrieves the current USD exchange rate relative to UAH.
    /// Should handle caching internally to minimize external API calls.
    /// </summary>
    /// <returns>Exchange rate</returns>
    Task<decimal> GetUsdRateAsync();
}