using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class PatientFileService(IDbContextFactory<AppDbContext> dbFactory) : IPatientFileService
{
    public async Task<IReadOnlyList<PatientCase>> SearchAsync(
        string facilityId,
        string? query,
        PatientCaseStatus? status,
        string? kindFilter,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var q = db.PatientCases.AsNoTracking().Where(x => x.FacilityId == facilityId);

        if (status.HasValue)
            q = q.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(kindFilter) && kindFilter != "الكل")
        {
            q = kindFilter switch
            {
                "إسعافي" => q.Where(x => x.EntryType == "إسعافي"),
                "عامة" => q.Where(x => x.Category == "عامة" && x.EntryType != "إسعافي"),
                "خاصة" => q.Where(x => x.Category == "خاصة"),
                _ => q
            };
        }

        var term = (query ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(term))
        {
            q = q.Where(x =>
                x.PatientName.Contains(term)
                || x.CaseNumber.Contains(term)
                || (x.NationalId != null && x.NationalId.Contains(term))
                || x.Governorate.Contains(term));
        }

        return await q
            .OrderByDescending(x => x.OpenedAt)
            .Take(200)
            .ToListAsync(ct);
    }

    public async Task<(int Total, int Prebooked, int Emergency, int Special, int InDepartment, int Returned)> GetStatsAsync(
        string facilityId,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rows = await db.PatientCases.AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .Select(x => new { x.EntryType, x.Category, x.Status })
            .ToListAsync(ct);

        return (
            rows.Count,
            rows.Count(x => x.EntryType == "حجز مسبق"),
            rows.Count(x => x.EntryType == "إسعافي"),
            rows.Count(x => x.Category == "خاصة"),
            rows.Count(x => x.Status == PatientCaseStatus.InDepartment),
            rows.Count(x => x.Status == PatientCaseStatus.ReturnedToReception));
    }
}
