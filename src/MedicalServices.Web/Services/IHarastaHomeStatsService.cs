namespace MedicalServices.Web.Services;

public interface IHarastaHomeStatsService
{
    Task<HarastaOpsSnapshot> GetOpsAsync(string facilityId, bool todayOnly, CancellationToken ct = default);

    Task<HarastaEventsSnapshot> GetEventsAsync(
        string facilityId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken ct = default);

    Task<IReadOnlyList<ClinicLiveRow>> GetClinicLiveAsync(string facilityId, CancellationToken ct = default);

    Task<ReceptionDaySnapshot> GetReceptionDayAsync(string facilityId, CancellationToken ct = default);
}

public sealed record HarastaOpsMetric(string Key, int Value, string Detail, string Tone);

public sealed record HarastaOpsSnapshot(IReadOnlyList<HarastaOpsMetric> Items);

public sealed record HarastaEventKind(string Key, string Tone, int Count);

public sealed record HarastaEventRow(
    string Kind,
    string Tone,
    string PatientName,
    string Label,
    DateTimeOffset At);

public sealed record HarastaDayBar(DateOnly Day, int Count);

public sealed record HarastaEventsSnapshot(
    IReadOnlyList<HarastaEventKind> Kinds,
    IReadOnlyList<HarastaEventRow> Rows,
    IReadOnlyList<HarastaDayBar> Daily,
    int Total);

public sealed record ClinicLiveRow(
    string Clinic,
    int Waiting,
    int TodayReferrals,
    string AvgWaitLabel,
    string MaxWaitLabel);

public sealed record ReceptionDestRow(string Destination, int Count, int Percent);

public sealed record ReceptionDaySnapshot(
    int EnteredToday,
    int AtReceptionNow,
    int DirectedToday,
    int EmergencyToday,
    int BookingArrivalsToday,
    int ReturnedToday,
    int PendingBookings,
    IReadOnlyList<ReceptionDestRow> Destinations);
