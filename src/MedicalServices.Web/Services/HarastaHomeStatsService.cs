using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using MedicalServices.Web.Services.Harasta;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class HarastaHomeStatsService(IDbContextFactory<AppDbContext> dbFactory) : IHarastaHomeStatsService
{
    private static readonly (string Key, string Tone)[] EventKinds =
    [
        ("فتح استمارة", "cyan"),
        ("إسعافي", "red"),
        ("توجيه لعيادة", "teal"),
        ("استدعاء", "green"),
        ("تحويل بين العيادات", "blue"),
        ("تحويل لقسم", "amber"),
        ("لم يحضر", "red"),
        ("إعادة للاستقبال", "violet"),
        ("خروج", "green")
    ];

    public async Task<HarastaOpsSnapshot> GetOpsAsync(
        string facilityId, bool todayOnly, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var cases = await db.PatientCases.AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .Select(x => new
            {
                x.EntryType,
                x.Category,
                x.SpecialType,
                x.Status,
                x.CurrentDepartment,
                x.OpenedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        var (dayStart, dayEnd) = LocalDayBoundsUtc();
        bool InR(DateTimeOffset t) => !todayOnly || (t >= dayStart && t < dayEnd);

        var opened = cases.Where(c => InR(c.OpenedAt)).ToList();
        var inDeptNow = cases.Count(c => c.Status == PatientCaseStatus.InDepartment);

        int Sent(string dept) => cases.Count(c =>
            c.CurrentDepartment == dept
            && (c.Status == PatientCaseStatus.InDepartment
                || c.Status == PatientCaseStatus.Closed)
            && InR(c.UpdatedAt));

        int Done(string dept) => cases.Count(c =>
            c.CurrentDepartment == dept
            && c.Status == PatientCaseStatus.Closed
            && InR(c.UpdatedAt));

        var clinicVisits = cases.Count(c =>
            (c.Status is PatientCaseStatus.InClinic or PatientCaseStatus.Closed
                or PatientCaseStatus.InDepartment or PatientCaseStatus.ReturnedToReception)
            && InR(c.UpdatedAt));

        var items = new List<HarastaOpsMetric>
        {
            new("أضابير مفتوحة", opened.Count,
                $"حجز {opened.Count(c => c.EntryType != "إسعافي")} · إسعافي {opened.Count(c => c.EntryType == "إسعافي")}", "cyan"),
            new("مراجعات العيادات", clinicVisits, "معاينة / تواجد", "blue"),
            new("العمليات", Done("العمليات"), $"منجزة من {Sent("العمليات")} محوّلة", "red"),
            new("المخبر", Done("المخبر"), $"نتيجة صادرة من {Sent("المخبر")}", "green"),
            new("الأشعة", Done("الأشعة"), $"تقرير صادر من {Sent("الأشعة")}", "teal"),
            new("الصيدلية", Done("الصيدلية"), $"وصفة مصروفة من {Sent("الصيدلية")}", "blue"),
            new("قبول الاستشفاء", Sent("أسرّة الاستشفاء"), $"خروج {Done("أسرّة الاستشفاء")}", "cyan"),
            new("قبول العناية", Sent("العناية"), $"خروج {Done("العناية")}", "red"),
            new("حالات خاصة", opened.Count(c => c.Category == "خاصة"),
                $"ابن شهيد {opened.Count(c => c.SpecialType == "ابن شهيد")} · بدون هوية {opened.Count(c => c.SpecialType == "بدون هوية")}", "amber"),
            new("إعادة توجيه",
                cases.Count(c => c.Status == PatientCaseStatus.ReturnedToReception && InR(c.UpdatedAt)),
                "أُعيد للاستقبال", "red"),
            new("خروج المرضى",
                cases.Count(c => c.Status == PatientCaseStatus.Closed && InR(c.UpdatedAt)),
                "إضبارة مغلقة", "green"),
            new("قيد المعالجة الآن", inDeptNow, "لدى جهات التحويل", "amber"),
        };

        return new HarastaOpsSnapshot(items);
    }

    public async Task<HarastaEventsSnapshot> GetEventsAsync(
        string facilityId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var cases = await db.PatientCases.AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .Select(x => new
            {
                x.PatientName,
                x.EntryType,
                x.Status,
                x.CurrentClinic,
                x.BookedClinic,
                x.CurrentDepartment,
                x.OpenedAt,
                x.UpdatedAt,
                x.CalledAt,
                x.CallCount,
                x.UpdatedBy
            })
            .ToListAsync(ct);

        var rows = new List<HarastaEventRow>();

        foreach (var c in cases)
        {
            if (c.OpenedAt >= fromUtc && c.OpenedAt < toUtc)
            {
                if (c.EntryType == "إسعافي")
                    rows.Add(Row("إسعافي", "red", c.PatientName, "دخول إسعافي — فتح إضبارة", c.OpenedAt));
                else
                    rows.Add(Row("فتح استمارة", "cyan", c.PatientName, "فتح استمارة عيادة", c.OpenedAt));
            }

            if (c.CalledAt is { } called && called >= fromUtc && called < toUtc)
            {
                var clinic = c.CurrentClinic ?? c.BookedClinic ?? "";
                rows.Add(Row("استدعاء", "green", c.PatientName,
                    $"استدعاء إلى {(clinic == "الإسعاف" ? clinic : "عيادة " + clinic)}"
                    + (c.CallCount > 1 ? $" (مرة {c.CallCount})" : ""),
                    called));
            }

            if (c.UpdatedAt < fromUtc || c.UpdatedAt >= toUtc)
                continue;

            // Skip status echo that coincides with open (same second)
            if (Math.Abs((c.UpdatedAt - c.OpenedAt).TotalSeconds) < 3
                && c.Status is PatientCaseStatus.AtReception or PatientCaseStatus.InClinic)
                continue;

            switch (c.Status)
            {
                case PatientCaseStatus.InClinic:
                    if (LooksLikeClinicTransfer(c.UpdatedBy, c.OpenedAt, c.UpdatedAt))
                        rows.Add(Row("تحويل بين العيادات", "blue", c.PatientName,
                            $"تحويل إلى عيادة {c.CurrentClinic ?? c.BookedClinic}", c.UpdatedAt));
                    else
                        rows.Add(Row("توجيه لعيادة", "teal", c.PatientName,
                            $"تحويل إلى عيادة {c.CurrentClinic ?? c.BookedClinic}", c.UpdatedAt));
                    break;
                case PatientCaseStatus.InDepartment:
                    rows.Add(Row("تحويل لقسم", "amber", c.PatientName,
                        $"تحويل إلى {c.CurrentDepartment}", c.UpdatedAt));
                    break;
                case PatientCaseStatus.ReturnedToReception:
                    rows.Add(Row("إعادة للاستقبال", "violet", c.PatientName,
                        "إعادته للاستقبال", c.UpdatedAt));
                    break;
                case PatientCaseStatus.Closed:
                    rows.Add(Row("خروج", "green", c.PatientName, "خروج المريض وإغلاق الإضبارة", c.UpdatedAt));
                    break;
            }
        }

        var noShows = await db.AppointmentBookings.AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.Status == BookingStatus.NoShow)
            .Select(x => new { x.PatientName, x.AppointmentDate, x.Clinic })
            .ToListAsync(ct);

        foreach (var b in noShows)
        {
            var localMidnight = b.AppointmentDate.ToDateTime(TimeOnly.MinValue);
            var at = new DateTimeOffset(localMidnight, TimeZoneInfo.Local.GetUtcOffset(localMidnight)).ToUniversalTime();
            if (at >= fromUtc && at < toUtc)
                rows.Add(Row("لم يحضر", "red", b.PatientName, $"لم يحضر — {b.Clinic}", at));
        }

        rows = rows.OrderByDescending(r => r.At).ToList();

        var kinds = EventKinds
            .Select(k => new HarastaEventKind(k.Key, k.Tone, rows.Count(r => r.Kind == k.Key)))
            .ToList();

        var spanDays = Math.Clamp((int)Math.Round((toUtc - fromUtc).TotalDays), 1, 14);
        var endLocal = toUtc.ToLocalTime().Date;
        var bars = new List<HarastaDayBar>();
        for (var i = spanDays - 1; i >= 0; i--)
        {
            var dayLocal = DateOnly.FromDateTime(endLocal.AddDays(-i));
            var localDt = dayLocal.ToDateTime(TimeOnly.MinValue);
            var start = new DateTimeOffset(localDt, TimeZoneInfo.Local.GetUtcOffset(localDt)).ToUniversalTime();
            var end = start.AddDays(1);
            bars.Add(new HarastaDayBar(dayLocal, rows.Count(r => r.At >= start && r.At < end)));
        }

        return new HarastaEventsSnapshot(kinds, rows.Take(80).ToList(), bars, rows.Count);
    }

    public async Task<ReceptionDaySnapshot> GetReceptionDayAsync(
        string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var (dayStart, dayEnd) = LocalDayBoundsUtc();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var cases = await db.PatientCases.AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .Select(x => new
            {
                x.EntryType,
                x.Status,
                x.BookedClinic,
                x.CurrentClinic,
                x.OpenedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        var openedToday = cases.Where(c => c.OpenedAt >= dayStart && c.OpenedAt < dayEnd).ToList();
        var atReception = cases.Count(c =>
            c.Status is PatientCaseStatus.AtReception or PatientCaseStatus.ReturnedToReception);

        // Destinations: cases opened today that left reception (or ER direct).
        var sentToday = openedToday
            .Where(c => c.Status is PatientCaseStatus.InClinic
                or PatientCaseStatus.InDepartment
                or PatientCaseStatus.Closed
                or PatientCaseStatus.ReturnedToReception)
            .ToList();

        var destGroups = sentToday
            .GroupBy(c => string.IsNullOrWhiteSpace(c.CurrentClinic)
                ? (string.IsNullOrWhiteSpace(c.BookedClinic) ? "غير محدد" : c.BookedClinic)
                : c.CurrentClinic!)
            .Select(g => (Dest: g.Key, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Dest, StringComparer.Ordinal)
            .ToList();

        var destTotal = destGroups.Sum(x => x.Count);
        var destinations = destGroups
            .Select(x => new ReceptionDestRow(
                x.Dest,
                x.Count,
                destTotal == 0 ? 0 : (int)Math.Round(100.0 * x.Count / destTotal)))
            .ToList();

        var pendingBookings = await db.AppointmentBookings.AsNoTracking()
            .CountAsync(b =>
                b.FacilityId == facilityId
                && b.Status == BookingStatus.Reserved
                && b.AppointmentDate >= today, ct);

        var bookingArrivals = openedToday.Count(c =>
            c.EntryType != "إسعافي"
            && c.OpenedAt >= dayStart);

        return new ReceptionDaySnapshot(
            EnteredToday: openedToday.Count,
            AtReceptionNow: atReception,
            DirectedToday: sentToday.Count,
            EmergencyToday: openedToday.Count(c => c.EntryType == "إسعافي"),
            BookingArrivalsToday: bookingArrivals,
            ReturnedToday: cases.Count(c =>
                c.Status == PatientCaseStatus.ReturnedToReception
                && c.UpdatedAt >= dayStart && c.UpdatedAt < dayEnd),
            PendingBookings: pendingBookings,
            Destinations: destinations);
    }

    public async Task<IReadOnlyList<ClinicLiveRow>> GetClinicLiveAsync(
        string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var (dayStart, dayEnd) = LocalDayBoundsUtc();
        var now = DateTimeOffset.UtcNow;

        var cases = await db.PatientCases.AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .Select(x => new
            {
                Clinic = x.CurrentClinic ?? x.BookedClinic,
                x.Status,
                x.OpenedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        var rows = new List<ClinicLiveRow>();
        foreach (var clinic in HarastaData.Clinics)
        {
            var waitingRows = cases
                .Where(c => c.Status == PatientCaseStatus.InClinic && c.Clinic == clinic)
                .ToList();
            var waiting = waitingRows.Count;

            var todayDone = cases.Count(c =>
                c.Clinic == clinic
                && c.Status is not (PatientCaseStatus.InClinic or PatientCaseStatus.AtReception)
                && c.UpdatedAt >= dayStart && c.UpdatedAt < dayEnd);
            var today = waiting + todayDone;

            var waits = waitingRows
                .Select(c => (now - c.UpdatedAt).TotalMilliseconds)
                .Where(ms => ms >= 0)
                .ToList();

            rows.Add(new ClinicLiveRow(
                clinic,
                waiting,
                today,
                FormatWait(waits.Count == 0 ? null : waits.Average()),
                FormatWait(waits.Count == 0 ? null : waits.Max())));
        }

        return rows;
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

    private static bool LooksLikeClinicTransfer(string? updatedBy, DateTimeOffset opened, DateTimeOffset updated) =>
        updatedBy is not null
        && updatedBy.Contains(" / ", StringComparison.Ordinal)
        && updated - opened > TimeSpan.FromMinutes(5);

    private static HarastaEventRow Row(
        string kind, string tone, string name, string label, DateTimeOffset at) =>
        new(kind, tone, name, label, at);

    private static (DateTimeOffset Start, DateTimeOffset End) LocalDayBoundsUtc()
    {
        var local = DateTime.Today;
        var start = new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local)).ToUniversalTime();
        return (start, start.AddDays(1));
    }
}
