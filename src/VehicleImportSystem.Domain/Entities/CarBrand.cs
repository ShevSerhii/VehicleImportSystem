namespace VehicleImportSystem.Domain.Entities;

/// <summary>
/// Represents a vehicle manufacturer (e.g., Volkswagen, BMW).
/// </summary>
public class CarBrand
{
    /// <summary>
    /// Unique identifier synchronized with the external Auto.ria system.
    /// This ID is manually assigned, not auto-generated.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The official name of the brand.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property: A brand can have multiple models.
    /// </summary>
    public ICollection<CarModel> Models { get; set; } = new List<CarModel>();
}