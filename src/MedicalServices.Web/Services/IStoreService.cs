using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IStoreService
{
    Task<IReadOnlyList<StoreItem>> ListAsync(string facilityId, CancellationToken ct = default);

    Task<StoreItem> AddAsync(string facilityId, AddStoreItemRequest request, string updatedBy, CancellationToken ct = default);

    Task<StoreItem> AdjustQuantityAsync(
        string facilityId,
        Guid itemId,
        int delta,
        string updatedBy,
        CancellationToken ct = default);
}

public sealed record AddStoreItemRequest(
    string StoreName,
    string Name,
    string Unit,
    int Quantity,
    int MinQuantity,
    DateOnly? ExpiryDate);
