using MedicalServices.Domain.Entities;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class UnitOpsService(IDbContextFactory<AppDbContext> dbFactory) : IUnitOpsService
{
    public async Task<IReadOnlyList<FacilityEvent>> ListEventsAsync(
        string facilityId, bool openOnly = false, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var q = db.FacilityEvents.AsNoTracking().Where(x => x.FacilityId == facilityId);
        if (openOnly)
            q = q.Where(x => x.Status == "مفتوح");
        return await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
    }

    public async Task<FacilityEvent> CreateEventAsync(
        string facilityId, CreateFacilityEventRequest request, string createdBy, CancellationToken ct = default)
    {
        var title = (request.Title ?? string.Empty).Trim();
        if (title.Length < 2)
            throw new InvalidOperationException("أدخل عنواناً واضحاً للحدث.");

        var kind = NormalizeOne(request.Kind, UnitOpsCatalog.EventKinds, "نوع الحدث غير صالح.");
        var severity = NormalizeOne(request.Severity, UnitOpsCatalog.EventSeverities, "درجة الخطورة غير صالحة.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = new FacilityEvent
        {
            FacilityId = facilityId,
            Title = title,
            Kind = kind,
            Severity = severity,
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            Status = "مفتوح",
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.FacilityEvents.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<FacilityEvent> CloseEventAsync(
        string facilityId, Guid eventId, string closedBy, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.FacilityEvents
            .FirstOrDefaultAsync(x => x.Id == eventId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الحدث غير موجود.");

        if (row.Status == "مغلق")
            return row;

        row.Status = "مغلق";
        row.ClosedBy = closedBy;
        row.ClosedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<IReadOnlyList<FacilityNeed>> ListNeedsAsync(
        string facilityId, bool openOnly = false, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var q = db.FacilityNeeds.AsNoTracking().Where(x => x.FacilityId == facilityId);
        if (openOnly)
            q = q.Where(x => x.Status == "مفتوح" || x.Status == "قيد التنفيذ");
        return await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
    }

    public async Task<FacilityNeed> CreateNeedAsync(
        string facilityId, CreateFacilityNeedRequest request, string createdBy, CancellationToken ct = default)
    {
        var title = (request.Title ?? string.Empty).Trim();
        if (title.Length < 2)
            throw new InvalidOperationException("أدخل وصف الاحتياج.");

        var category = NormalizeOne(request.Category, UnitOpsCatalog.NeedCategories, "فئة الاحتياج غير صالحة.");
        var priority = NormalizeOne(request.Priority, UnitOpsCatalog.NeedPriorities, "أولوية غير صالحة.");
        var qty = Math.Max(1, request.Quantity);

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = new FacilityNeed
        {
            FacilityId = facilityId,
            Title = title,
            Category = category,
            Priority = priority,
            Quantity = qty,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Status = "مفتوح",
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.FacilityNeeds.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<FacilityNeed> UpdateNeedStatusAsync(
        string facilityId, Guid needId, string status, string updatedBy, CancellationToken ct = default)
    {
        var allowed = new[] { "مفتوح", "قيد التنفيذ", "مُلبّى", "ملغى" };
        if (!allowed.Contains(status))
            throw new InvalidOperationException("حالة الاحتياج غير صالحة.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.FacilityNeeds
            .FirstOrDefaultAsync(x => x.Id == needId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("طلب الاحتياج غير موجود.");

        row.Status = status;
        row.UpdatedBy = updatedBy;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return row;
    }

    private static string NormalizeOne(string? value, IReadOnlyList<string> allowed, string error)
    {
        var v = (value ?? string.Empty).Trim();
        if (!allowed.Contains(v))
            throw new InvalidOperationException(error);
        return v;
    }
}
