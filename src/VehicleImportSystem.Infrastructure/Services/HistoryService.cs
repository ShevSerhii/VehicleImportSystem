using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Application.Mappings;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// Implementation of IHistoryService for managing calculation history records.
/// Provides functionality to retrieve, delete individual records, or clear user history.
/// </summary>
public class HistoryService : IHistoryService
{
    private readonly IAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the HistoryService.
    /// </summary>
    /// <param name="context">The database context for data access.</param>
    public HistoryService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CalculationRecordDto>> GetUserHistoryAsync(string userDeviceId, CancellationToken ct)
    {
        var records = await _context.CustomsCalculation
            .AsNoTracking()
            .Include(r => r.Brand)
            .Include(r => r.Model)
            .Where(r => r.UserDeviceId == userDeviceId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return records.Select(r => r.ToDto()).ToList();
    }

    public async Task<bool> DeleteRecordAsync(int id, CancellationToken ct)
    {
        var record = await _context.CustomsCalculation.FindAsync(new object[] { id }, ct);
        if (record == null) return false;

        _context.CustomsCalculation.Remove(record);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task ClearUserHistoryAsync(string userDeviceId, CancellationToken ct)
    {
        await _context.CustomsCalculation
            .Where(r => r.UserDeviceId == userDeviceId)
            .ExecuteDeleteAsync(ct);
    }
}