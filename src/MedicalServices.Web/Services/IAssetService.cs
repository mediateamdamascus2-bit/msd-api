using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;

namespace MedicalServices.Web.Services;

public interface IAssetService
{
    Task<IReadOnlyList<FacilityAsset>> ListAsync(string facilityId, CancellationToken ct = default);

    Task<FacilityAsset> AddAsync(string facilityId, AddFacilityAssetRequest request, string updatedBy, CancellationToken ct = default);

    Task<FacilityAsset> SetStatusAsync(
        string facilityId,
        Guid assetId,
        FacilityAssetStatus status,
        string updatedBy,
        CancellationToken ct = default);

    Task<FacilityAsset> CycleStatusAsync(string facilityId, Guid assetId, string updatedBy, CancellationToken ct = default);
}

public sealed record AddFacilityAssetRequest(
    string Code,
    string Name,
    string Department,
    int Count,
    string? Note);
