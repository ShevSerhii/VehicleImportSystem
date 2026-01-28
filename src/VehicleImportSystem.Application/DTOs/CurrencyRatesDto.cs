namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Data transfer object representing currency exchange rates.
/// Contains EUR and USD rates relative to UAH with timestamp.
/// </summary>
public class CurrencyRatesDto
{
    /// <summary>
    /// EUR to UAH exchange rate.
    /// </summary>
    public decimal Eur { get; set; }

    /// <summary>
    /// USD to UAH exchange rate.
    /// </summary>
    public decimal Usd { get; set; }

    /// <summary>
    /// Timestamp when the rates were retrieved (ISO 8601 format).
    /// </summary>
    public string Date { get; set; } = string.Empty;
}

