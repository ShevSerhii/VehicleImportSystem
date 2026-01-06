namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Data transfer object representing a historical calculation record for display in user history.
/// Contains essential information about a past customs calculation.
/// </summary>
public class CalculationRecordDto
{
    /// <summary>
    /// Unique identifier of the calculation record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Timestamp when the calculation was performed (UTC).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Name of the vehicle brand (null if brand was not found).
    /// </summary>
    public string? BrandName { get; set; }

    /// <summary>
    /// Name of the vehicle model (null if model was not found).
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Manufacturing year of the vehicle.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Original price of the vehicle abroad in EUR.
    /// </summary>
    public decimal PriceInEur { get; set; }

    /// <summary>
    /// Final "turnkey" price including all taxes (Price + Taxes).
    /// </summary>
    public decimal TotalTurnkeyPrice { get; set; }

    /// <summary>
    /// Calculated potential profit (Market Price - Turnkey Price).
    /// Positive value indicates profit, negative indicates loss.
    /// </summary>
    public decimal PotentialProfit { get; set; }
}