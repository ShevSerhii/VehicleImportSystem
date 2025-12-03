namespace VehicleImportSystem.Domain.Enums;

/// <summary>
/// Represents the type of vehicle engine/fuel.
/// Critical for calculating excise tax rates.
/// </summary>
public enum FuelType
{
    /// <summary>
    /// Standard petrol (gasoline) engine.
    /// </summary>
    Petrol = 1,

    /// <summary>
    /// Diesel engine.
    /// </summary>
    Diesel,

    /// <summary>
    /// Fully electric vehicle (BEV). Has 0% customs duty and VAT.
    /// </summary>
    Electric,

    /// <summary>
    /// Hybrid electric vehicle (HEV/PHEV). Has a fixed excise rate.
    /// </summary>
    Hybrid
}