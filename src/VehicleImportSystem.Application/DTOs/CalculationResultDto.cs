namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Detailed calculation response sent back to the client.
/// Contains tax breakdown, totals, and analytical comparison.
/// </summary>
public record CalculationResultDto
{

    /// <summary>
    /// Import Duty (10% or 0% for EV).
    /// </summary>
    public decimal ImportDuty { get; set; }

    /// <summary>
    /// Excise Tax calculated based on engine volume and age.
    /// </summary>
    public decimal ExciseTax { get; set; }

    /// <summary>
    /// Value Added Tax (20% on sum of Price+Duty+Excise).
    /// </summary>
    public decimal Vat { get; set; }

    /// <summary>
    /// Pension Fund fee (3-5% based on total value).
    /// </summary>
    public decimal PensionFund { get; set; }

    /// <summary>
    /// Total sum of all taxes to be paid at customs.
    /// </summary>
    public decimal TotalCustomsClearance { get; set; }

    /// <summary>
    /// Final price of the vehicle "turnkey" (Price + Taxes).
    /// </summary>
    public decimal TotalVehicleCost { get; set; } 


    /// <summary>
    /// Average market price of a similar vehicle in Ukraine (from Auto.ria).
    /// </summary>
    public decimal MarketPrice { get; set; }

    /// <summary>
    /// Calculated savings (MarketPrice - TotalVehicleCost).
    /// </summary>
    public decimal PotentialProfit { get; set; }

    /// <summary>
    /// Helper flag for UI: true if importing is cheaper than buying locally.
    /// </summary>
    public bool IsProfitable { get; set; }

    /// <summary>
    /// The currency exchange rate used for the calculation.
    /// </summary>
    public decimal CurrencyRateUsed { get; set; }
}