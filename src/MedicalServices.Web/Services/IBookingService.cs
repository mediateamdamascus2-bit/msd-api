using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IBookingService
{
    Task<IReadOnlyDictionary<string, int>> GetDayAvailabilityAsync(
        string facilityId, DateOnly date, CancellationToken ct = default);

    Task<IReadOnlyList<AppointmentBooking>> ListAsync(
        string facilityId, string entity, string adminUnit, CancellationToken ct = default);

    /// <summary>Find previously booked people (by national ID or name) to reuse their form.</summary>
    Task<IReadOnlyList<BookingPatientProfile>> SearchPatientsAsync(
        string facilityId,
        string query,
        string? entity = null,
        string? adminUnit = null,
        CancellationToken ct = default);

    /// <summary>Reserved bookings from today onward (for reception inbox).</summary>
    Task<IReadOnlyList<AppointmentBooking>> ListPendingReservedAsync(
        string facilityId, CancellationToken ct = default);

    Task<IReadOnlyList<AppointmentBooking>> CreateAsync(
        string facilityId, CreateBookingsRequest request, CancellationToken ct = default);

    Task<AppointmentBooking> CancelAsync(
        string facilityId, Guid bookingId, CancellationToken ct = default);

    Task<AppointmentBooking> MarkNoShowAsync(
        string facilityId, Guid bookingId, CancellationToken ct = default);

    Task<AppointmentBooking> MarkArrivedAsync(
        string facilityId, Guid bookingId, CancellationToken ct = default);
}

public sealed record BookingPersonRequest(
    string PatientName,
    string? NationalId,
    int? Age,
    string Governorate,
    string? Residence,
    string Category,
    string? SpecialType);

public sealed record BookingPatientProfile(
    string PatientName,
    string? NationalId,
    int? Age,
    string Governorate,
    string? Residence,
    string Category,
    string? SpecialType,
    string? LastClinic,
    DateOnly? LastAppointmentDate,
    int BookingCount);

public sealed record CreateBookingsRequest(
    string Entity,
    string AdminUnit,
    string Clinic,
    DateOnly AppointmentDate,
    string PreferredSlot,
    string BookedBy,
    IReadOnlyList<BookingPersonRequest> People);
