using VehicleImportSystem.Application.DTOs;

namespace VehicleImportSystem.Application.Interfaces;

/// <summary>
/// Contract for the main calculation logic.
/// </summary>
public interface ICustomsCalculatorService
{
    /// <summary>
    /// Calculates customs duties and taxes asynchronously.
    /// </summary>
    /// <param name="request">Input data (Price, Engine, etc.)</param>
    /// <param name="userDeviceId">Unique ID of the guest user (for history)</param>
    /// <returns>Full calculation result with analytics</returns>
    Task<CalculationResultDto> CalculateAsync(CalculationRequest request, string userDeviceId);
}