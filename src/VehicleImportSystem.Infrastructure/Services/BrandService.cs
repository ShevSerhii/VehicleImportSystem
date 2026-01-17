using Microsoft.EntityFrameworkCore;
using VehicleImportSystem.Application.DTOs;
using VehicleImportSystem.Application.Interfaces;
using VehicleImportSystem.Application.Mappings;

namespace VehicleImportSystem.Infrastructure.Services;

/// <summary>
/// Implementation of IBrandService for retrieving vehicle brand and model data from the database.
/// </summary>
public class BrandService : IBrandService
{
    private readonly IAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the BrandService.
    /// </summary>
    /// <param name="context">The database context for data access.</param>
    public BrandService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync(CancellationToken ct)
    {
        return await _context.CarBrands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => b.ToDto())
            .ToListAsync(ct);
    }
}