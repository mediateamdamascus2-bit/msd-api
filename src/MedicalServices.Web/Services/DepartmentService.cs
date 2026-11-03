using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using MedicalServices.Web.Services.Harasta;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class DepartmentService(IDbContextFactory<AppDbContext> dbFactory) : IDepartmentService
{
    public async Task<IReadOnlyDictionary<string, int>> GetQueueCountsAsync(
        string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rows = await db.PatientCases
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.Status == PatientCaseStatus.InDepartment)
            .GroupBy(x => x.CurrentDepartment!)
            .Select(g => new { Dept = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return HarastaData.Departments.ToDictionary(
            d => d,
            d => rows.FirstOrDefault(r => r.Dept == d)?.Count ?? 0);
    }

    public async Task<IReadOnlyList<PatientCase>> GetQueueAsync(
        string facilityId, string department, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PatientCases
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId
                        && x.Status == PatientCaseStatus.InDepartment
                        && x.CurrentDepartment == department)
            .OrderBy(x => x.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DeptLiveCard>> GetLiveBoardAsync(
        string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var localToday = DateTime.Today;
        var dayStart = new DateTimeOffset(localToday, TimeZoneInfo.Local.GetUtcOffset(localToday)).ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        var active = await db.PatientCases
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId
                        && x.Status == PatientCaseStatus.InDepartment
                        && x.CurrentDepartment != null)
            .Select(x => new { x.CurrentDepartment, x.UpdatedAt })
            .ToListAsync(ct);

        var todayClosed = await db.PatientCases
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId
                        && x.Status == PatientCaseStatus.Closed
                        && x.CurrentDepartment != null
                        && x.UpdatedAt >= dayStart
                        && x.UpdatedAt < dayEnd)
            .Select(x => new { x.CurrentDepartment, x.UpdatedAt })
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var cards = new List<DeptLiveCard>();

        foreach (var name in HarastaData.Departments)
        {
            var tone = HarastaData.DepartmentTone.GetValueOrDefault(name, "cyan");
            var capacity = HarastaData.DepartmentCapacity.GetValueOrDefault(name);
            var presentRows = active.Where(x => x.CurrentDepartment == name).OrderBy(x => x.UpdatedAt).ToList();
            var present = presentRows.Count;
            var occupied = capacity is int cap ? Math.Min(cap, present) : 0;
            var free = capacity is int c ? Math.Max(0, c - present) : 0;
            var queued = capacity is int cq ? Math.Max(0, present - cq) : present;

            var todayCount = presentRows.Count(x => x.UpdatedAt >= dayStart && x.UpdatedAt < dayEnd)
                + todayClosed.Count(x => x.CurrentDepartment == name);

            var waitSources = capacity is not null
                ? presentRows.Skip(capacity.Value).ToList()
                : presentRows;
            var waits = waitSources
                .Select(x => (now - x.UpdatedAt).TotalMilliseconds)
                .Where(ms => ms >= 0)
                .ToList();

            cards.Add(new DeptLiveCard(
                name,
                tone,
                capacity,
                present,
                occupied,
                free,
                queued,
                todayCount,
                FormatWait(waits.Count == 0 ? null : waits.Average()),
                FormatWait(waits.Count == 0 ? null : waits.Max())));
        }

        return cards;
    }

    private static string FormatWait(double? ms)
    {
        if (ms is null or <= 0) return "—";
        var totalMin = (int)Math.Round(ms.Value / 60000.0);
        if (totalMin < 1) return "<1 د";
        if (totalMin < 60) return $"{totalMin} د";
        var h = totalMin / 60;
        var m = totalMin % 60;
        return m == 0 ? $"{h} س" : $"{h} س {m} د";
    }

    public async Task<PatientCase> CompleteAsync(
        string facilityId,
        Guid caseId,
        string resultNote,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await RequireInDeptAsync(db, facilityId, caseId, ct);

        if (!string.IsNullOrWhiteSpace(resultNote))
            row.DepartmentNote = resultNote.Trim();

        row.Status = PatientCaseStatus.Closed;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<PatientCase> ReturnToClinicAsync(
        string facilityId,
        Guid caseId,
        string note,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await RequireInDeptAsync(db, facilityId, caseId, ct);

        var clinic = row.CurrentClinic ?? row.BookedClinic;
        if (string.IsNullOrWhiteSpace(clinic))
            throw new InvalidOperationException("لا توجد عيادة للعودة إليها.");

        if (!string.IsNullOrWhiteSpace(note))
            row.DepartmentNote = note.Trim();

        row.CurrentDepartment = null;
        row.Status = PatientCaseStatus.InClinic;
        row.CurrentClinic = clinic;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await db.SaveChangesAsync(ct);
        return row;
    }

    private static async Task<PatientCase> RequireInDeptAsync(
        AppDbContext db, string facilityId, Guid caseId, CancellationToken ct)
    {
        var row = await db.PatientCases
            .FirstOrDefaultAsync(x => x.Id == caseId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الإضبارة غير موجودة.");

        if (row.Status != PatientCaseStatus.InDepartment)
            throw new InvalidOperationException("الإضبارة ليست في قسم تحويل حالياً.");

        return row;
    }
}
