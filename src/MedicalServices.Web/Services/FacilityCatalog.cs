using MedicalServices.Domain.Entities;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class FacilityCatalog(IDbContextFactory<AppDbContext> dbFactory) : IFacilityCatalog
{
    public async Task<IReadOnlyList<Facility>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Facilities.AsNoTracking()
            .OrderBy(f => f.Type)
            .ThenBy(f => f.NameAr)
            .ToListAsync(ct);
    }

    public async Task<Facility?> FindAsync(string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Facilities.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == facilityId, ct);
    }
}
