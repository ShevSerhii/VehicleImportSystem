namespace VehicleImportSystem.Application.DTOs;

/// <summary>
/// Data transfer object representing average market price response.
/// </summary>
public record AveragePriceDto
{
    /// <summary>
    /// Average market price in USD.
    /// </summary>
    public decimal PriceUsd { get; set; }
}

