using MedicalServices.Domain.Entities;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class StoreService(IDbContextFactory<AppDbContext> dbFactory) : IStoreService
{
    public async Task<IReadOnlyList<StoreItem>> ListAsync(string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.StoreItems
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .OrderBy(x => x.StoreName)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<StoreItem> AddAsync(
        string facilityId,
        AddStoreItemRequest request,
        string updatedBy,
        CancellationToken ct = default)
    {
        var store = (request.StoreName ?? string.Empty).Trim();
        var name = (request.Name ?? string.Empty).Trim();
        var unit = (request.Unit ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(store))
            throw new InvalidOperationException("حدد المستودع.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("اسم الصنف مطلوب.");
        if (string.IsNullOrWhiteSpace(unit))
            throw new InvalidOperationException("وحدة القياس مطلوبة.");
        if (request.Quantity < 0)
            throw new InvalidOperationException("الكمية لا يمكن أن تكون سالبة.");
        if (request.MinQuantity < 0)
            throw new InvalidOperationException("الحد الأدنى لا يمكن أن يكون سالباً.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (await db.StoreItems.AnyAsync(
                x => x.FacilityId == facilityId
                     && x.StoreName == store
                     && x.Name == name,
                ct))
            throw new InvalidOperationException("هذا الصنف موجود مسبقاً في المستودع.");

        var row = new StoreItem
        {
            FacilityId = facilityId,
            StoreName = store,
            Name = name,
            Unit = unit,
            Quantity = request.Quantity,
            MinQuantity = request.MinQuantity,
            ExpiryDate = request.ExpiryDate,
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = updatedBy
        };

        db.StoreItems.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<StoreItem> AdjustQuantityAsync(
        string facilityId,
        Guid itemId,
        int delta,
        string updatedBy,
        CancellationToken ct = default)
    {
        if (delta == 0)
            throw new InvalidOperationException("أدخل كمية أكبر من صفر.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.StoreItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الصنف غير موجود.");

        var next = row.Quantity + delta;
        if (next < 0)
            throw new InvalidOperationException("الكمية المتاحة غير كافية للصرف.");

        row.Quantity = next;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;

        await db.SaveChangesAsync(ct);
        return row;
    }
}
