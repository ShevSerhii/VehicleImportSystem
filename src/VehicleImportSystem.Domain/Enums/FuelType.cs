using System.ComponentModel;

namespace VehicleImportSystem.Domain.Enums;

/// <summary>
/// Represents the type of vehicle engine/fuel.
/// Values are explicitly mapped to Auto.RIA API IDs to ensure correct filtering.
/// </summary>
public enum FuelType
{
    /// <summary>
    /// Auto.RIA ID: 1
    /// </summary>
    [Description("Petrol")]
    Petrol = 1,

    /// <summary>
    /// Auto.RIA ID: 2
    /// </summary>
    [Description("Diesel")]
    Diesel = 2,

    /// <summary>
    /// Auto.RIA ID: 3. (Gas only - rare)
    /// </summary>
    [Description("Gas")]
    Gas = 3,

    /// <summary>
    /// Auto.RIA ID: 4. (Gas/Petrol - converted cars, popular in Ukraine)
    /// </summary>
    [Description("Gas/Petrol")]
    GasPetrol = 4,

    /// <summary>
    /// Auto.RIA ID: 5
    /// </summary>
    [Description("Hybrid")]
    Hybrid = 5,

    /// <summary>
    /// Auto.RIA ID: 6
    /// </summary>
    [Description("Electric")]
    Electric = 6
}