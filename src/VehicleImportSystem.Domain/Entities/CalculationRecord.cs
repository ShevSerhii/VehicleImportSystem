using VehicleImportSystem.Domain.Enums;

namespace VehicleImportSystem.Domain.Entities;

/// <summary>
/// Represents a historical record of a customs calculation performed by a user.
/// Contains a snapshot of input data, selected vehicle, and calculated financial results.
/// </summary>
public class CalculationRecord
{
    /// <summary>
    /// Unique identifier for the calculation record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Anonymous identifier of the user (Guest ID stored in local storage).
    /// Allows grouping history without user registration.
    /// </summary>
    public string UserDeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the calculation was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    /// <summary>
    /// Foreign Key to the selected Brand.
    /// </summary>
    public int? BrandId { get; set; }

    /// <summary>
    /// Navigation property to the Brand entity.
    /// </summary>
    public CarBrand? Brand { get; set; }

    /// <summary>
    /// Foreign Key to the selected Model.
    /// </summary>
    public int? ModelId { get; set; }

    /// <summary>
    /// Navigation property to the Model entity.
    /// </summary>
    public CarModel? Model { get; set; }

    /// <summary>
    /// Manufacturing year of the vehicle. 
    /// Used to calculate the age coefficient for Excise Tax.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Type of fuel/engine (Petrol, Diesel, Electric, etc.).
    /// Determines the tax formula.
    /// </summary>
    public FuelType FuelType { get; set; }

    /// <summary>
    /// Engine displacement (cm3) for ICE or Battery Capacity (kWh) for Electric cars.
    /// </summary>
    public int EngineCapacity { get; set; }

    /// <summary>
    /// Original price of the vehicle abroad (Netto) in EUR.
    /// </summary>
    public decimal PriceInEur { get; set; }

    /// <summary>
    /// Total sum of all taxes (Duty + Excise + VAT + Pension Fund).
    /// </summary>
    public decimal TotalCustomsCost { get; set; }

    /// <summary>
    /// Final price "turnkey" (Car Price + Taxes).
    /// </summary>
    public decimal TotalTurnkeyPrice { get; set; }

    /// <summary>
    /// Snapshot of the average market price on Auto.ria at the moment of calculation.
    /// Used to track market dynamics historically.
    /// </summary>
    public decimal MarketPriceSnapshot { get; set; }

    /// <summary>
    /// Calculated savings (Market Price - Turnkey Price).
    /// A positive value indicates profit, negative indicates loss.
    /// </summary>
    public decimal PotentialProfit { get; set; }
}