using VehicleImportSystem.Application.DTOs;

namespace VehicleImportSystem.Application.Interfaces;

/// <summary>
/// Service for managing calculation history records.
/// Allows users to retrieve, delete individual records, or clear their entire history.
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Retrieves all calculation records for a specific user, ordered by creation date (newest first).
    /// </summary>
    /// <param name="userDeviceId">Unique identifier of the user's device.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of calculation records with brand and model names included.</returns>
    Task<List<CalculationRecordDto>> GetUserHistoryAsync(string userDeviceId, CancellationToken ct);

    /// <summary>
    /// Deletes a specific calculation record by its ID.
    /// </summary>
    /// <param name="id">Unique identifier of the calculation record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the record was found and deleted, false otherwise.</returns>
    Task<bool> DeleteRecordAsync(int id, CancellationToken ct);

    /// <summary>
    /// Deletes all calculation records for a specific user.
    /// </summary>
    /// <param name="userDeviceId">Unique identifier of the user's device.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ClearUserHistoryAsync(string userDeviceId, CancellationToken ct);
}