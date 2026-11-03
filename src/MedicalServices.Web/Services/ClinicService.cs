using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using MedicalServices.Web.Services.Harasta;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class ClinicService(
    IDbContextFactory<AppDbContext> dbFactory,
    IReceptionNotifier receptionNotifier,
    ICallBoard callBoard) : IClinicService
{
    public async Task<IReadOnlyDictionary<string, int>> GetQueueCountsAsync(
        string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rows = await db.PatientCases
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.Status == PatientCaseStatus.InClinic)
            .GroupBy(x => x.CurrentClinic ?? x.BookedClinic)
            .Select(g => new { Clinic = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return HarastaData.Clinics.ToDictionary(
            c => c,
            c => rows.FirstOrDefault(r => r.Clinic == c)?.Count ?? 0);
    }

    public async Task<IReadOnlyList<PatientCase>> GetQueueAsync(
        string facilityId, string clinic, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PatientCases
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId
                        && x.Status == PatientCaseStatus.InClinic
                        && (x.CurrentClinic == clinic || (x.CurrentClinic == null && x.BookedClinic == clinic)))
            .OrderBy(x => x.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<PatientCase> CompleteVisitAsync(
        string facilityId,
        Guid caseId,
        ClinicCompleteRequest request,
        string updatedBy,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Doctor))
            throw new InvalidOperationException("أدخل اسم الطبيب المعاين.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await RequireInClinicAsync(db, facilityId, caseId, ct);

        row.Status = PatientCaseStatus.Closed;
        row.CalledAt = null;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = $"{request.Doctor.Trim()} / {updatedBy}";
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<PatientCase> CallPatientAsync(
        string facilityId,
        Guid caseId,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await RequireInClinicAsync(db, facilityId, caseId, ct);
        var clinic = row.CurrentClinic ?? row.BookedClinic;

        row.CallCount += 1;
        row.CalledAt = DateTimeOffset.UtcNow;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await db.SaveChangesAsync(ct);

        callBoard.Publish(new PatientCallEvent(
            facilityId,
            row.Id,
            row.CaseNumber,
            row.PatientName,
            clinic,
            row.CallCount,
            row.CalledAt.Value));

        return row;
    }

    public async Task<PatientCase> ReturnToReceptionAsync(
        string facilityId,
        Guid caseId,
        ClinicReturnRequest request,
        string updatedBy,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Doctor))
            throw new InvalidOperationException("أدخل اسم الطبيب المعاين.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("اكتب سبب الإعادة إلى الاستقبال.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await RequireInClinicAsync(db, facilityId, caseId, ct);
        var fromClinic = row.CurrentClinic ?? row.BookedClinic;

        row.Status = PatientCaseStatus.ReturnedToReception;
        row.CurrentClinic = null;
        row.CalledAt = null;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = $"{request.Doctor.Trim()} / {updatedBy}";
        await db.SaveChangesAsync(ct);

        receptionNotifier.NotifyReturnedToReception(
            facilityId, row.PatientName, row.CaseNumber, fromClinic, request.Reason.Trim());

        return row;
    }

    public async Task<PatientCase> TransferClinicAsync(
        string facilityId,
        Guid caseId,
        string toClinic,
        string doctor,
        string updatedBy,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(doctor))
            throw new InvalidOperationException("أدخل اسم الطبيب المعاين.");
        toClinic = (toClinic ?? string.Empty).Trim();
        if (!HarastaData.Clinics.Contains(toClinic))
            throw new InvalidOperationException("العيادة المستهدفة غير صالحة.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await RequireInClinicAsync(db, facilityId, caseId, ct);

        row.CurrentClinic = toClinic;
        row.BookedClinic = toClinic;
        row.Status = PatientCaseStatus.InClinic;
        row.CalledAt = null;
        row.CallCount = 0;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = $"{doctor.Trim()} / {updatedBy}";
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<PatientCase> ReferToDepartmentAsync(
        string facilityId,
        Guid caseId,
        string department,
        string doctor,
        string? note,
        string updatedBy,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(doctor))
            throw new InvalidOperationException("أدخل اسم الطبيب المعاين.");
        department = (department ?? string.Empty).Trim();
        if (!HarastaData.Departments.Contains(department))
            throw new InvalidOperationException("قسم التحويل غير صالح.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await RequireInClinicAsync(db, facilityId, caseId, ct);

        row.CurrentDepartment = department;
        row.DepartmentNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        row.Status = PatientCaseStatus.InDepartment;
        row.CalledAt = null;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = $"{doctor.Trim()} / {updatedBy}";
        await db.SaveChangesAsync(ct);
        return row;
    }

    private static async Task<PatientCase> RequireInClinicAsync(
        AppDbContext db, string facilityId, Guid caseId, CancellationToken ct)
    {
        var row = await db.PatientCases
            .FirstOrDefaultAsync(x => x.Id == caseId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الإضبارة غير موجودة.");

        if (row.Status != PatientCaseStatus.InClinic)
            throw new InvalidOperationException("الإضبارة ليست في العيادة حالياً.");

        return row;
    }
}
