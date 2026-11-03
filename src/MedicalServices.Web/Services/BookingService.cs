using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class BookingService(
    IDbContextFactory<AppDbContext> dbFactory,
    IReceptionNotifier receptionNotifier) : IBookingService
{
    private static readonly char[] CodeLetters = "ABCDEFGHJKLMNPRSTUVWXYZ".ToCharArray();

    public async Task<IReadOnlyDictionary<string, int>> GetDayAvailabilityAsync(
        string facilityId, DateOnly date, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var counts = await db.AppointmentBookings
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId
                        && x.AppointmentDate == date
                        && (x.Status == BookingStatus.Reserved || x.Status == BookingStatus.Arrived))
            .GroupBy(x => x.Clinic)
            .Select(g => new { Clinic = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return BookingCatalog.BookableClinics.ToDictionary(
            c => c,
            c => Math.Max(0, BookingCatalog.DailyCap - (counts.FirstOrDefault(x => x.Clinic == c)?.Count ?? 0)));
    }

    public async Task<IReadOnlyList<AppointmentBooking>> ListAsync(
        string facilityId, string entity, string adminUnit, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var q = db.AppointmentBookings.AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.Entity == entity);

        if (BookingCatalog.RequiresAdminUnit(entity))
            q = q.Where(x => x.AdminUnit == adminUnit);

        return await q
            .OrderBy(x => x.AppointmentDate)
            .ThenBy(x => x.Slot)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BookingPatientProfile>> SearchPatientsAsync(
        string facilityId,
        string query,
        string? entity = null,
        string? adminUnit = null,
        CancellationToken ct = default)
    {
        query = (query ?? string.Empty).Trim();
        if (query.Length < 2)
            return [];

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var q = db.AppointmentBookings.AsNoTracking()
            .Where(x => x.FacilityId == facilityId);

        if (!string.IsNullOrWhiteSpace(entity))
            q = q.Where(x => x.Entity == entity);

        if (!string.IsNullOrWhiteSpace(adminUnit) && BookingCatalog.RequiresAdminUnit(entity ?? string.Empty))
            q = q.Where(x => x.AdminUnit == adminUnit);

        var digits = new string(query.Where(char.IsDigit).ToArray());
        if (digits.Length >= 4)
            q = q.Where(x => x.NationalId != null && x.NationalId.Contains(digits));
        else
            q = q.Where(x => x.PatientName.Contains(query));

        var rows = await q
            .OrderByDescending(x => x.CreatedAt)
            .Take(120)
            .ToListAsync(ct);

        var profiles = new List<BookingPatientProfile>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var key = !string.IsNullOrWhiteSpace(row.NationalId)
                ? $"id:{row.NationalId}"
                : $"name:{row.PatientName}|{row.Age}";
            if (!seen.Add(key))
                continue;

            var count = !string.IsNullOrWhiteSpace(row.NationalId)
                ? rows.Count(x => x.NationalId == row.NationalId)
                : rows.Count(x => string.Equals(x.PatientName, row.PatientName, StringComparison.OrdinalIgnoreCase));

            profiles.Add(new BookingPatientProfile(
                row.PatientName,
                row.NationalId,
                row.Age,
                row.Governorate,
                row.Residence,
                row.Category,
                row.SpecialType,
                row.Clinic,
                row.AppointmentDate,
                count));

            if (profiles.Count >= 12)
                break;
        }

        return profiles;
    }

    public async Task<IReadOnlyList<AppointmentBooking>> CreateAsync(
        string facilityId, CreateBookingsRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.BookedBy))
            throw new InvalidOperationException("أدخل اسم الموظف المسؤول عن الحجز.");

        if (request.AppointmentDate.DayOfWeek == DayOfWeek.Friday)
            throw new InvalidOperationException("يوم الجمعة عطلة — اختر يوماً آخر.");

        if (!BookingCatalog.BookableClinics.Contains(request.Clinic))
            throw new InvalidOperationException("العيادة غير صالحة.");

        if (!BookingCatalog.Slots.Contains(request.PreferredSlot))
            throw new InvalidOperationException("الموعد غير صالح.");

        var entity = request.Entity?.Trim() ?? string.Empty;
        if (!BookingCatalog.Entities.Contains(entity))
            throw new InvalidOperationException("الجهة غير صالحة.");

        var admin = (request.AdminUnit ?? string.Empty).Trim();
        if (BookingCatalog.RequiresAdminUnit(entity) && string.IsNullOrWhiteSpace(admin))
            throw new InvalidOperationException(BookingCatalog.MissingAdminMessage(entity));

        if (request.People is null || request.People.Count == 0)
            throw new InvalidOperationException("أضف شخصاً واحداً على الأقل.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var dayRows = await db.AppointmentBookings
            .Where(x => x.FacilityId == facilityId
                        && x.Clinic == request.Clinic
                        && x.AppointmentDate == request.AppointmentDate
                        && (x.Status == BookingStatus.Reserved || x.Status == BookingStatus.Arrived))
            .ToListAsync(ct);

        var left = BookingCatalog.DailyCap - dayRows.Count;
        if (request.People.Count > left)
            throw new InvalidOperationException($"السعة المتبقية لهذه العيادة في هذا اليوم {left} فقط.");

        var slotUsed = BookingCatalog.Slots.ToDictionary(
            s => s,
            s => dayRows.Count(x => x.Slot == s));

        var reservedCodes = await db.AppointmentBookings
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.Status == BookingStatus.Reserved)
            .Select(x => x.Code)
            .ToListAsync(ct);
        var taken = new HashSet<string>(reservedCodes, StringComparer.OrdinalIgnoreCase);

        var created = new List<AppointmentBooking>();
        var si = -1;
        for (var i = 0; i < BookingCatalog.Slots.Count; i++)
        {
            if (BookingCatalog.Slots[i] == request.PreferredSlot)
            {
                si = i;
                break;
            }
        }
        if (si < 0) si = 0;

        for (var i = 0; i < request.People.Count; i++)
        {
            var p = request.People[i];
            var tag = request.People.Count > 1 ? $" (الشخص {i + 1})" : string.Empty;
            var name = PatientNameRules.RequireTripleName(p.PatientName, tag);

            var noId = p.Category == "خاصة" && p.SpecialType == "بدون هوية";
            var nationalId = string.IsNullOrWhiteSpace(p.NationalId)
                ? null
                : new string(p.NationalId.Where(char.IsDigit).ToArray());

            if (!noId && (nationalId is null || nationalId.Length != 11))
                throw new InvalidOperationException("الرقم الوطني 11 رقماً" + tag);

            if (noId) nationalId = null;

            if (!string.IsNullOrEmpty(nationalId) &&
                dayRows.Any(x => x.NationalId == nationalId))
                throw new InvalidOperationException("هذا الشخص محجوز مسبقاً على نفس العيادة والتاريخ" + tag);

            if (!string.IsNullOrEmpty(nationalId) &&
                created.Any(x => x.NationalId == nationalId))
                throw new InvalidOperationException("رقم وطني مكرر داخل الطلب" + tag);

            var guard = 0;
            while (slotUsed[BookingCatalog.Slots[si]] >= BookingCatalog.PerSlot && guard++ < BookingCatalog.Slots.Count)
                si = (si + 1) % BookingCatalog.Slots.Count;

            slotUsed[BookingCatalog.Slots[si]]++;
            var code = NewCode(taken);
            taken.Add(code);

            var row = new AppointmentBooking
            {
                FacilityId = facilityId,
                Code = code,
                Entity = entity,
                AdminUnit = BookingCatalog.RequiresAdminUnit(entity) ? admin : string.Empty,
                Clinic = request.Clinic,
                AppointmentDate = request.AppointmentDate,
                Slot = BookingCatalog.Slots[si],
                PatientName = name,
                NationalId = nationalId,
                Age = p.Age is >= 0 and <= 120 ? p.Age : null,
                Governorate = string.IsNullOrWhiteSpace(p.Governorate) ? "دمشق" : p.Governorate.Trim(),
                Residence = string.IsNullOrWhiteSpace(p.Residence) ? null : p.Residence.Trim(),
                Category = p.Category == "خاصة" ? "خاصة" : "عامة",
                SpecialType = p.Category == "خاصة" ? p.SpecialType : null,
                Status = BookingStatus.Reserved,
                BookedBy = request.BookedBy.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            created.Add(row);
            dayRows.Add(row);
        }

        db.AppointmentBookings.AddRange(created);
        await db.SaveChangesAsync(ct);

        // Notify Harasta reception UI (live toast + refresh).
        receptionNotifier.NotifyBookingCreated(facilityId, created);

        return created;
    }

    public async Task<IReadOnlyList<AppointmentBooking>> ListPendingReservedAsync(
        string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await db.AppointmentBookings
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId
                        && x.AppointmentDate >= today
                        && x.Status == BookingStatus.Reserved)
            .OrderBy(x => x.AppointmentDate)
            .ThenBy(x => x.Slot)
            .ThenBy(x => x.Code)
            .ToListAsync(ct);
    }

    public async Task<AppointmentBooking> CancelAsync(
        string facilityId, Guid bookingId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.AppointmentBookings
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الحجز غير موجود.");

        if (row.Status != BookingStatus.Reserved)
            throw new InvalidOperationException("لا يمكن إلغاء هذا الحجز.");

        row.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<AppointmentBooking> MarkNoShowAsync(
        string facilityId, Guid bookingId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.AppointmentBookings
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الحجز غير موجود.");

        if (row.Status != BookingStatus.Reserved)
            throw new InvalidOperationException("لا يمكن تحديث هذا الحجز.");

        row.Status = BookingStatus.NoShow;
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<AppointmentBooking> MarkArrivedAsync(
        string facilityId, Guid bookingId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.AppointmentBookings
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الحجز غير موجود.");

        if (row.Status != BookingStatus.Reserved)
            throw new InvalidOperationException("الحجز ليس بانتظار الحضور.");

        row.Status = BookingStatus.Arrived;
        await db.SaveChangesAsync(ct);
        return row;
    }

    private static string NewCode(HashSet<string> taken)
    {
        for (var i = 0; i < 200; i++)
        {
            var code = $"{CodeLetters[Random.Shared.Next(CodeLetters.Length)]}{Random.Shared.Next(100):D2}";
            if (taken.Add(code))
                return code;
        }

        throw new InvalidOperationException("تعذر توليد رمز حجز فريد.");
    }
}
