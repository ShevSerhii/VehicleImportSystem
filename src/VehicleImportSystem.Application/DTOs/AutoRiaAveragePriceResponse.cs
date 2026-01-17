using System.Text.Json.Serialization;

namespace VehicleImportSystem.Infrastructure.Services.AutoRia;

/// <summary>
/// Internal DTO for parsing the average price response
/// </summary>
public record AutoRiaAveragePriceResponse
{
    [JsonPropertyName("interQuartileMean")]
    public double InterQuartileMean { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}