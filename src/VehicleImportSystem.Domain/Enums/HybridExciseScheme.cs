namespace VehicleImportSystem.Domain.Enums;

/// <summary>
/// How excise is calculated for hybrid vehicles (пп. 215.3.5-1 ПКУ).
/// </summary>
public enum HybridExciseScheme
{
    /// <summary>
    /// Fixed 100 EUR per vehicle (УКТ ЗЕД 8703 10 18 00, 8703 80 10 90, 8703 80 90 90, 8703 90 00 00).
    /// </summary>
    FixedRate = 0,

    /// <summary>
    /// Petrol/diesel formula by ICE volume and age (typical plug-in hybrid).
    /// </summary>
    ByIceEngine = 1
}
