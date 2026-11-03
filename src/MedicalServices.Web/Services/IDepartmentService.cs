using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IDepartmentService
{
    Task<IReadOnlyDictionary<string, int>> GetQueueCountsAsync(string facilityId, CancellationToken ct = default);

    Task<IReadOnlyList<PatientCase>> GetQueueAsync(
        string facilityId, string department, CancellationToken ct = default);

    Task<IReadOnlyList<DeptLiveCard>> GetLiveBoardAsync(string facilityId, CancellationToken ct = default);

    Task<PatientCase> CompleteAsync(
        string facilityId,
        Guid caseId,
        string resultNote,
        string updatedBy,
        CancellationToken ct = default);

    Task<PatientCase> ReturnToClinicAsync(
        string facilityId,
        Guid caseId,
        string note,
        string updatedBy,
        CancellationToken ct = default);
}

public sealed record DeptLiveCard(
    string Name,
    string Tone,
    int? Capacity,
    int Present,
    int Occupied,
    int Free,
    int Queued,
    int TodayReferrals,
    string AvgWaitLabel,
    string MaxWaitLabel);
