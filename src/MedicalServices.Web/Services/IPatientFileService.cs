using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;

namespace MedicalServices.Web.Services;

public interface IPatientFileService
{
    Task<IReadOnlyList<PatientCase>> SearchAsync(
        string facilityId,
        string? query,
        PatientCaseStatus? status,
        string? kindFilter,
        CancellationToken ct = default);

    Task<(int Total, int Prebooked, int Emergency, int Special, int InDepartment, int Returned)> GetStatsAsync(
        string facilityId,
        CancellationToken ct = default);
}
