using MedicalServices.Domain.Entities;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class AdmissionsService(IDbContextFactory<AppDbContext> dbFactory) : IAdmissionsService
{
    public async Task<DailyAdmission> GetOrCreateTodayAsync(string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var day = DateOnly.FromDateTime(DateTime.UtcNow);

        var existing = await db.DailyAdmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FacilityId == facilityId && x.Day == day, ct);

        if (existing is not null)
            return existing;

        var created = new DailyAdmission
        {
            FacilityId = facilityId,
            Day = day,
            UpdatedBy = "system",
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.DailyAdmissions.Add(created);
        await db.SaveChangesAsync(ct);
        return created;
    }

    public async Task<DailyAdmission> UpsertTodayAsync(
        string facilityId,
        AdmissionsUpdate update,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var day = DateOnly.FromDateTime(DateTime.UtcNow);

        var row = await db.DailyAdmissions
            .FirstOrDefaultAsync(x => x.FacilityId == facilityId && x.Day == day, ct);

        if (row is null)
        {
            row = new DailyAdmission
            {
                FacilityId = facilityId,
                Day = day
            };
            db.DailyAdmissions.Add(row);
        }

        row.AdmissionsCount = Math.Max(0, update.AdmissionsCount);
        row.OccupiedBeds = Math.Max(0, update.OccupiedBeds);
        row.AvailableBeds = Math.Max(0, update.AvailableBeds);
        row.TransfersCount = Math.Max(0, update.TransfersCount);
        row.Notes = string.IsNullOrWhiteSpace(update.Notes) ? null : update.Notes.Trim();
        row.UpdatedBy = updatedBy;
        row.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return row;
    }
}
