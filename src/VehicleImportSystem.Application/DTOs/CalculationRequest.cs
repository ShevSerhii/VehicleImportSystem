using VehicleImportSystem.Domain.Enums;

namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Data transfer object representing the user's input for calculation.
/// Contains all technical parameters required for tax formulas.
/// </summary>
public sealed record CalculationRequest
{

    /// <summary>
    /// Manufacturing year of the vehicle (e.g., 2018).
    /// Used to calculate the age coefficient.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Type of engine (Petrol, Diesel, Electric, Hybrid).
    /// Determines which tax formula to apply.
    /// </summary>
    public FuelType FuelType { get; set; }

    /// <summary>
    /// Engine displacement in cm3 (for ICE) or Battery capacity in kWh (for EV).
    /// </summary>
    public int EngineCapacity { get; set; }

    /// <summary>
    /// The price of the car abroad (Netto) in EUR.
    /// </summary>
    public decimal PriceInEur { get; set; }

    /// <summary>
    /// ID of the selected Brand (from Auto.ria).
    /// </summary>
    public int MarkId { get; set; }

    /// <summary>
    /// ID of the selected Model (from Auto.ria).
    /// </summary>
    public int ModelId { get; set; }

    /// <summary>
    /// For <see cref="FuelType.Hybrid"/>: fixed 100 EUR or ICE volume formula (пп. 215.3.5-1 ПКУ).
    /// </summary>
    public HybridExciseScheme? HybridExciseScheme { get; set; }

    /// <summary>
    /// For hybrid with <see cref="HybridExciseScheme.ByIceEngine"/>: petrol or diesel ICE rates.
    /// </summary>
    public FuelType? HybridIceFuelType { get; set; }

    /// <summary>
    /// For electric vehicles: share of customs value (0–1) exempt from VAT under transitional rules
    /// (first taxable event before 01.01.2026). 0 = full 20% VAT.
    /// </summary>
    public decimal EvVatExemptShare { get; set; }
}