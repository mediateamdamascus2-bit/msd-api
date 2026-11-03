using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IReceptionService
{
    Task<IReadOnlyList<PatientCase>> GetQueueAsync(string facilityId, CancellationToken ct = default);

    /// <summary>Prior visitors/bookings — reuse form fields without retyping.</summary>
    Task<IReadOnlyList<ReceptionPatientProfile>> SearchKnownPatientsAsync(
        string facilityId, string query, CancellationToken ct = default);

    Task<PatientCase> OpenAsync(string facilityId, OpenPatientCaseRequest request, string openedBy, CancellationToken ct = default);
    Task<PatientCase> ArriveFromBookingAsync(string facilityId, Guid bookingId, string openedBy, CancellationToken ct = default);
    Task<PatientCase> UpdateAtReceptionAsync(string facilityId, Guid caseId, UpdatePatientCaseRequest request, string updatedBy, CancellationToken ct = default);
    Task<PatientCase> SendToClinicAsync(string facilityId, Guid caseId, string? clinic, string updatedBy, CancellationToken ct = default);
}

public sealed record ReceptionPatientProfile(
    string PatientName,
    string? NationalId,
    int? Age,
    string Governorate,
    string? Residence,
    string Category,
    string? SpecialType,
    string? AdminUnit,
    string? BookedClinic,
    string Source,
    int VisitCount);

public sealed record OpenPatientCaseRequest(
    string EntryType,
    string Category,
    string? SpecialType,
    string? SpecialNote,
    string? NationalId,
    string PatientName,
    int? Age,
    string? Residence,
    string Governorate,
    string? AdminUnit,
    string BookedClinic);

public sealed record UpdatePatientCaseRequest(
    string Category,
    string? SpecialType,
    string? SpecialNote,
    string? NationalId,
    string PatientName,
    int? Age,
    string? Residence,
    string Governorate,
    string? AdminUnit,
    string BookedClinic);
