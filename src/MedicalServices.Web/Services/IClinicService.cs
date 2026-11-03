using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IClinicService
{
    Task<IReadOnlyDictionary<string, int>> GetQueueCountsAsync(string facilityId, CancellationToken ct = default);

    Task<IReadOnlyList<PatientCase>> GetQueueAsync(
        string facilityId, string clinic, CancellationToken ct = default);

    Task<PatientCase> CompleteVisitAsync(
        string facilityId,
        Guid caseId,
        ClinicCompleteRequest request,
        string updatedBy,
        CancellationToken ct = default);

    Task<PatientCase> ReturnToReceptionAsync(
        string facilityId,
        Guid caseId,
        ClinicReturnRequest request,
        string updatedBy,
        CancellationToken ct = default);

    Task<PatientCase> TransferClinicAsync(
        string facilityId,
        Guid caseId,
        string toClinic,
        string doctor,
        string updatedBy,
        CancellationToken ct = default);

    Task<PatientCase> ReferToDepartmentAsync(
        string facilityId,
        Guid caseId,
        string department,
        string doctor,
        string? note,
        string updatedBy,
        CancellationToken ct = default);

    Task<PatientCase> CallPatientAsync(
        string facilityId,
        Guid caseId,
        string updatedBy,
        CancellationToken ct = default);
}

public sealed record ClinicCompleteRequest(string Doctor, string? Complaint, string? Note);

public sealed record ClinicReturnRequest(string Doctor, string Reason);
