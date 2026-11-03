using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IAdmissionsService
{
    Task<DailyAdmission> GetOrCreateTodayAsync(string facilityId, CancellationToken ct = default);
    Task<DailyAdmission> UpsertTodayAsync(string facilityId, AdmissionsUpdate update, string updatedBy, CancellationToken ct = default);
}

public sealed record AdmissionsUpdate(
    int AdmissionsCount,
    int OccupiedBeds,
    int AvailableBeds,
    int TransfersCount,
    string? Notes);
