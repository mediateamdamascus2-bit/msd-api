using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IUnitOpsService
{
    Task<IReadOnlyList<FacilityEvent>> ListEventsAsync(string facilityId, bool openOnly = false, CancellationToken ct = default);
    Task<FacilityEvent> CreateEventAsync(string facilityId, CreateFacilityEventRequest request, string createdBy, CancellationToken ct = default);
    Task<FacilityEvent> CloseEventAsync(string facilityId, Guid eventId, string closedBy, CancellationToken ct = default);

    Task<IReadOnlyList<FacilityNeed>> ListNeedsAsync(string facilityId, bool openOnly = false, CancellationToken ct = default);
    Task<FacilityNeed> CreateNeedAsync(string facilityId, CreateFacilityNeedRequest request, string createdBy, CancellationToken ct = default);
    Task<FacilityNeed> UpdateNeedStatusAsync(string facilityId, Guid needId, string status, string updatedBy, CancellationToken ct = default);
}

public sealed record CreateFacilityEventRequest(
    string Title,
    string Kind,
    string Severity,
    string? Details);

public sealed record CreateFacilityNeedRequest(
    string Title,
    string Category,
    string Priority,
    int Quantity,
    string? Notes);
