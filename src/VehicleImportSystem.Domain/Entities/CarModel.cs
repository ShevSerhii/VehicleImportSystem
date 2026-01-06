namespace VehicleImportSystem.Domain.Entities;

/// <summary>
/// Represents a specific model belonging to a manufacturer (e.g., Golf, X5).
/// </summary>
public class CarModel
{
    /// <summary>
    /// Unique identifier synchronized with the external Auto.ria system.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the model.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Foreign Key linking to the manufacturer (CarBrand).
    /// </summary>
    public int BrandId { get; set; }

    /// <summary>
    /// Navigation property to the parent Brand.
    /// </summary>
    public CarBrand? Brand { get; set; }
}
