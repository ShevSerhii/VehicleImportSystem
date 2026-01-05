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
    Petrol,

    /// <summary>
    /// Diesel engine.
    /// </summary>
    Diesel,

    /// <summary>
    /// Fully electric vehicle (BEV).
    /// </summary>
    Electric,

    /// <summary>
    /// Hybrid electric vehicle (HEV/PHEV). Has a fixed excise rate.
    /// </summary>
    Hybrid
}