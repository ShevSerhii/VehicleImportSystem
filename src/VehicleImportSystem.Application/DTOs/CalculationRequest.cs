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
    /// Unique identifier of the user's device (UUID from LocalStorage).
    /// Used to link calculation history to a guest user.
    /// </summary>
    public string UserDeviceId { get; set; } = string.Empty;
}