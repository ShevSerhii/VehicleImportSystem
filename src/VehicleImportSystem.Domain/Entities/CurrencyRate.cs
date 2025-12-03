namespace VehicleImportSystem.Domain.Entities;

/// <summary>
/// Represents a currency exchange rate record (e.g., NBU rate).
/// Used to cache rates locally to reduce external API calls.
/// </summary>
public class CurrencyRate
{
    /// <summary>
    /// Internal primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ISO 4217 currency code (e.g., "EUR", "USD").
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// The exchange rate relative to UAH (Ukrainian Hryvnia).
    /// High precision is required for financial calculations.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// The date for which this exchange rate is valid.
    /// </summary>
    public DateTime ExchangeDate { get; set; }
}