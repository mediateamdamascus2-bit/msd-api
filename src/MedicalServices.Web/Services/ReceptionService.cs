using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class ReceptionService(IDbContextFactory<AppDbContext> dbFactory) : IReceptionService
{
    public async Task<IReadOnlyList<PatientCase>> GetQueueAsync(string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PatientCases
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId &&
                        (x.Status == PatientCaseStatus.AtReception ||
                         x.Status == PatientCaseStatus.ReturnedToReception))
            .OrderBy(x => x.OpenedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReceptionPatientProfile>> SearchKnownPatientsAsync(
        string facilityId, string query, CancellationToken ct = default)
    {
        query = (query ?? string.Empty).Trim();
        if (query.Length < 2)
            return [];

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var digits = new string(query.Where(char.IsDigit).ToArray());

        var caseQuery = db.PatientCases.AsNoTracking().Where(x => x.FacilityId == facilityId);
        var bookQuery = db.AppointmentBookings.AsNoTracking().Where(x => x.FacilityId == facilityId);

        if (digits.Length >= 4)
        {
            caseQuery = caseQuery.Where(x => x.NationalId != null && x.NationalId.Contains(digits));
            bookQuery = bookQuery.Where(x => x.NationalId != null && x.NationalId.Contains(digits));
        }
        else
        {
            caseQuery = caseQuery.Where(x => x.PatientName.Contains(query));
            bookQuery = bookQuery.Where(x => x.PatientName.Contains(query));
        }

        var cases = await caseQuery.OrderByDescending(x => x.OpenedAt).Take(80).ToListAsync(ct);
        var books = await bookQuery.OrderByDescending(x => x.CreatedAt).Take(80).ToListAsync(ct);

        var profiles = new List<ReceptionPatientProfile>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(
            string name, string? nationalId, int? age, string governorate, string? residence,
            string category, string? special, string? adminUnit, string? clinic, string source)
        {
            var key = !string.IsNullOrWhiteSpace(nationalId) ? $"id:{nationalId}" : $"name:{name}|{age}";
            if (!seen.Add(key))
                return;

            var visitCount = !string.IsNullOrWhiteSpace(nationalId)
                ? cases.Count(x => x.NationalId == nationalId) + books.Count(x => x.NationalId == nationalId)
                : cases.Count(x => string.Equals(x.PatientName, name, StringComparison.OrdinalIgnoreCase))
                  + books.Count(x => string.Equals(x.PatientName, name, StringComparison.OrdinalIgnoreCase));

            profiles.Add(new ReceptionPatientProfile(
                name, nationalId, age, governorate, residence, category, special,
                adminUnit, clinic, source, Math.Max(1, visitCount)));
        }

        // Prefer prior clinic visits (richer dossier) then bookings.
        foreach (var c in cases)
        {
            Add(c.PatientName, c.NationalId, c.Age, c.Governorate, c.Residence,
                c.Category, c.SpecialType, c.AdminUnit, c.BookedClinic ?? c.CurrentClinic, "إضبارة سابقة");
            if (profiles.Count >= 12) break;
        }

        foreach (var b in books)
        {
            if (profiles.Count >= 12) break;
            var affiliation = string.IsNullOrWhiteSpace(b.AdminUnit)
                ? b.Entity
                : $"{b.Entity} — {b.AdminUnit}";
            Add(b.PatientName, b.NationalId, b.Age, b.Governorate, b.Residence,
                b.Category, b.SpecialType, affiliation, b.Clinic, "حجز سابق");
        }

        return profiles;
    }

    public async Task<PatientCase> OpenAsync(
        string facilityId,
        OpenPatientCaseRequest request,
        string openedBy,
        CancellationToken ct = default)
    {
        var emergency = string.Equals(request.EntryType, "إسعافي", StringComparison.Ordinal);
        var name = PatientNameRules.RequireTripleName(request.PatientName);
        var noId = string.Equals(request.Category, "خاصة", StringComparison.Ordinal)
                   && string.Equals(request.SpecialType, "بدون هوية", StringComparison.Ordinal);

        var nationalId = string.IsNullOrWhiteSpace(request.NationalId)
            ? null
            : new string(request.NationalId.Where(char.IsDigit).ToArray());

        if (!noId && !emergency)
        {
            if (nationalId is null || nationalId.Length != 11)
                throw new InvalidOperationException("الرقم الوطني يجب أن يكون 11 رقماً — أو اختر حالة خاصة «بدون هوية».");
        }

        if (noId)
            nationalId = null;

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (!string.IsNullOrEmpty(nationalId))
        {
            var openDup = await db.PatientCases.AnyAsync(
                x => x.FacilityId == facilityId
                     && x.NationalId == nationalId
                     && x.Status != PatientCaseStatus.Closed,
                ct);
            if (openDup)
                throw new InvalidOperationException("يوجد إضبارة مفتوحة لهذا الرقم الوطني.");
        }

        var next = await db.PatientCases
            .Where(x => x.FacilityId == facilityId)
            .CountAsync(ct) + 1;

        var clinic = emergency ? "الإسعاف" : (request.BookedClinic?.Trim() ?? string.Empty);
        if (string.IsNullOrWhiteSpace(clinic))
            throw new InvalidOperationException("العيادة مطلوبة.");

        var now = DateTimeOffset.UtcNow;
        var row = new PatientCase
        {
            FacilityId = facilityId,
            CaseNumber = $"HR-{next:D4}",
            EntryType = emergency ? "إسعافي" : "حجز مسبق",
            Category = string.Equals(request.Category, "خاصة", StringComparison.Ordinal) ? "خاصة" : "عامة",
            SpecialType = string.Equals(request.Category, "خاصة", StringComparison.Ordinal)
                ? request.SpecialType
                : null,
            SpecialNote = string.IsNullOrWhiteSpace(request.SpecialNote) ? null : request.SpecialNote.Trim(),
            NationalId = nationalId,
            PatientName = name,
            Age = request.Age is >= 0 and <= 120 ? request.Age : null,
            Residence = string.IsNullOrWhiteSpace(request.Residence) ? null : request.Residence.Trim(),
            Governorate = string.IsNullOrWhiteSpace(request.Governorate) ? "ريف دمشق" : request.Governorate.Trim(),
            AdminUnit = emergency || string.IsNullOrWhiteSpace(request.AdminUnit)
                ? null
                : request.AdminUnit.Trim(),
            BookedClinic = clinic,
            CurrentClinic = emergency ? clinic : null,
            Status = emergency ? PatientCaseStatus.InClinic : PatientCaseStatus.AtReception,
            OpenedAt = now,
            OpenedBy = openedBy,
            UpdatedAt = now,
            UpdatedBy = openedBy
        };

        db.PatientCases.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<PatientCase> ArriveFromBookingAsync(
        string facilityId,
        Guid bookingId,
        string openedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var booking = await db.AppointmentBookings
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الحجز غير موجود.");

        if (booking.Status != BookingStatus.Reserved)
            throw new InvalidOperationException("هذا الحجز لم يعد بانتظار الحضور.");

        if (booking.AppointmentDate != today)
            throw new InvalidOperationException("يظهر في الاستقبال فقط حجوزات اليوم.");

        var noId = booking.Category == "خاصة" && booking.SpecialType == "بدون هوية";
        var nationalId = string.IsNullOrWhiteSpace(booking.NationalId) ? null : booking.NationalId;
        if (!noId && (nationalId is null || nationalId.Length != 11))
            throw new InvalidOperationException("الرقم الوطني في الحجز غير صالح لفتح الاستمارة.");

        if (!string.IsNullOrEmpty(nationalId))
        {
            var openDup = await db.PatientCases.AnyAsync(
                x => x.FacilityId == facilityId
                     && x.NationalId == nationalId
                     && x.Status != PatientCaseStatus.Closed,
                ct);
            if (openDup)
                throw new InvalidOperationException("يوجد إضبارة مفتوحة لهذا الرقم الوطني.");
        }

        var next = await db.PatientCases.Where(x => x.FacilityId == facilityId).CountAsync(ct) + 1;
        var now = DateTimeOffset.UtcNow;
        var adminLabel = string.IsNullOrWhiteSpace(booking.AdminUnit)
            ? booking.Entity
            : $"{booking.Entity} — {booking.AdminUnit}";

        var row = new PatientCase
        {
            FacilityId = facilityId,
            CaseNumber = $"HR-{next:D4}",
            EntryType = "حجز مسبق",
            Category = booking.Category == "خاصة" ? "خاصة" : "عامة",
            SpecialType = booking.Category == "خاصة" ? booking.SpecialType : null,
            NationalId = noId ? null : nationalId,
            PatientName = booking.PatientName,
            Age = booking.Age,
            Residence = booking.Residence,
            Governorate = booking.Governorate,
            AdminUnit = adminLabel,
            BookedClinic = booking.Clinic,
            Status = PatientCaseStatus.AtReception,
            OpenedAt = now,
            OpenedBy = openedBy,
            UpdatedAt = now,
            UpdatedBy = openedBy
        };

        booking.Status = BookingStatus.Arrived;
        db.PatientCases.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<PatientCase> UpdateAtReceptionAsync(
        string facilityId,
        Guid caseId,
        UpdatePatientCaseRequest request,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.PatientCases
            .FirstOrDefaultAsync(x => x.Id == caseId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الإضبارة غير موجودة.");

        if (row.Status is not (PatientCaseStatus.AtReception or PatientCaseStatus.ReturnedToReception))
            throw new InvalidOperationException("لا يمكن تعديل الإضبارة إلا وهي في الاستقبال.");

        var name = PatientNameRules.RequireTripleName(request.PatientName);

        var clinic = (request.BookedClinic ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(clinic))
            throw new InvalidOperationException("العيادة مطلوبة.");

        var category = string.IsNullOrWhiteSpace(request.Category) ? "عامة" : request.Category.Trim();
        var noId = category == "خاصة" && request.SpecialType == "بدون هوية";
        string? nationalId = null;
        if (!noId)
        {
            var nid = (request.NationalId ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(nid))
            {
                if (nid.Length != 11 || !nid.All(char.IsDigit))
                    throw new InvalidOperationException("الرقم الوطني يجب أن يكون 11 رقماً.");
                nationalId = nid;
            }
        }

        row.Category = category;
        row.SpecialType = category == "خاصة" ? request.SpecialType?.Trim() : null;
        row.SpecialNote = string.IsNullOrWhiteSpace(request.SpecialNote) ? null : request.SpecialNote.Trim();
        row.NationalId = nationalId;
        row.PatientName = name;
        row.Age = request.Age is >= 0 and <= 120 ? request.Age : null;
        row.Residence = string.IsNullOrWhiteSpace(request.Residence) ? null : request.Residence.Trim();
        row.Governorate = string.IsNullOrWhiteSpace(request.Governorate) ? row.Governorate : request.Governorate.Trim();
        row.AdminUnit = string.IsNullOrWhiteSpace(request.AdminUnit) ? null : request.AdminUnit.Trim();
        row.BookedClinic = clinic;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;

        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<PatientCase> SendToClinicAsync(
        string facilityId,
        Guid caseId,
        string? clinic,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.PatientCases
            .FirstOrDefaultAsync(x => x.Id == caseId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الإضبارة غير موجودة.");

        if (row.Status is not (PatientCaseStatus.AtReception or PatientCaseStatus.ReturnedToReception))
            throw new InvalidOperationException("الإضبارة ليست في طابور الاستقبال.");

        var target = string.IsNullOrWhiteSpace(clinic) ? row.BookedClinic : clinic.Trim();
        if (string.IsNullOrWhiteSpace(target))
            throw new InvalidOperationException("حدد العيادة المستهدفة.");

        row.CurrentClinic = target;
        row.BookedClinic = target;
        row.Status = PatientCaseStatus.InClinic;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;

        await db.SaveChangesAsync(ct);
        return row;
    }
}
