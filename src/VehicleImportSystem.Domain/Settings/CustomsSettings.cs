namespace VehicleImportSystem.Domain.Settings;

/// <summary>
/// Configuration model that holds all tax rates and legislative thresholds.
/// These values are loaded dynamically from the appsettings.json file.
/// </summary>
public class CustomsSettings
{
    /// <summary>
    /// Standard Import Duty rate (usually 0.10 for 10%).
    /// </summary>
    public decimal ImportDutyRate { get; set; }

    /// <summary>
    /// Value Added Tax rate (usually 0.20 for 20%).
    /// </summary>
    public decimal VatRate { get; set; }

    /// <summary>
    /// Excise rate for Petrol engines up to 3000 cm3 (e.g., 50 EUR).
    /// </summary>
    public decimal PetrolRateSmall { get; set; }

    /// <summary>
    /// Excise rate for Petrol engines over 3000 cm3 (e.g., 100 EUR).
    /// </summary>
    public decimal PetrolRateLarge { get; set; }

    /// <summary>
    /// Excise rate for Diesel engines up to 3500 cm3 (e.g., 75 EUR).
    /// </summary>
    public decimal DieselRateSmall { get; set; }

    /// <summary>
    /// Excise rate for Diesel engines over 3500 cm3 (e.g., 150 EUR).
    /// </summary>
    public decimal DieselRateLarge { get; set; }

    /// <summary>
    /// Fixed excise rate for Hybrid vehicles (e.g., 100 EUR).
    /// </summary>
    public decimal HybridRate { get; set; }

    /// <summary>
    /// Excise rate per kWh for Electric vehicles (e.g., 1 EUR).
    /// </summary>
    public decimal ElectricRate { get; set; }

    /// <summary>
    /// Maximum age of the vehicle taken into account for the excise formula (usually 15 years).
    /// </summary>
    public int MaxExciseAge { get; set; }

    /// <summary>
    /// Upper limit for the 3% Pension Fund rate (in UAH).
    /// </summary>
    public decimal PensionThresholdTier1 { get; set; }

    /// <summary>
    /// Upper limit for the 4% Pension Fund rate (in UAH).
    /// </summary>
    public decimal PensionThresholdTier2 { get; set; }

    /// <summary>
    /// Low Pension Fund rate (e.g., 0.03).
    /// </summary>
    public decimal PensionRateLow { get; set; }

    /// <summary>
    /// Medium Pension Fund rate (e.g., 0.04).
    /// </summary>
    public decimal PensionRateMedium { get; set; }

    /// <summary>
    /// High Pension Fund rate (e.g., 0.05).
    /// </summary>
    public decimal PensionRateHigh { get; set; }

    /// <summary>
    /// Volume threshold for Petrol engines (usually 3000 cm3).
    /// Engines below or equal use the Small rate, above use the Large rate.
    /// </summary>
    public int PetrolVolumeThreshold { get; set; }

    /// <summary>
    /// Volume threshold for Diesel engines (usually 3500 cm3).
    /// </summary>
    public int DieselVolumeThreshold { get; set; }
}