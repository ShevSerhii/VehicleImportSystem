using System.Text.Json.Serialization;

namespace VehicleImportSystem.Infrastructure.Services.AutoRia;

/// <summary>
/// Internal DTO for parsing the models list response
/// </summary>
internal record AutoRiaModelResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

/// <summary>
/// Internal DTO for parsing the average price response
/// </summary>
internal record AutoRiaAveragePriceResponse
{
    [JsonPropertyName("interQuartileMean")]
    public double InterQuartileMean { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}