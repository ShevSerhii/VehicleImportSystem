using System.Text.Json.Serialization;

namespace VehicleImportSystem.Infrastructure.DTOs;

/// <summary>
/// Data transfer object for deserializing Auto.ria brand seed data from JSON.
/// Used during database initialization to populate the CarBrands table.
/// </summary>
public record AutoRiaItemDto
{
    /// <summary>
    /// Brand name from Auto.ria API.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brand ID from Auto.ria API (used as primary key in our database).
    /// </summary>
    [JsonPropertyName("value")]
    public int Value { get; set; }
}