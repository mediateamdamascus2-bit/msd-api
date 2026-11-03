using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class AssetService(IDbContextFactory<AppDbContext> dbFactory) : IAssetService
{
    public async Task<IReadOnlyList<FacilityAsset>> ListAsync(string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.FacilityAssets
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .OrderBy(x => x.Department)
            .ThenBy(x => x.Code)
            .ToListAsync(ct);
    }

    public async Task<FacilityAsset> AddAsync(
        string facilityId,
        AddFacilityAssetRequest request,
        string updatedBy,
        CancellationToken ct = default)
    {
        var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant();
        var name = (request.Name ?? string.Empty).Trim();
        var dept = (request.Department ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("رمز الأصل مطلوب.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("اسم الأصل مطلوب.");
        if (string.IsNullOrWhiteSpace(dept))
            throw new InvalidOperationException("القسم مطلوب.");
        if (request.Count < 1)
            throw new InvalidOperationException("العدد يجب أن يكون 1 على الأقل.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (await db.FacilityAssets.AnyAsync(x => x.FacilityId == facilityId && x.Code == code, ct))
            throw new InvalidOperationException("رمز الأصل مستخدم مسبقاً.");

        var row = new FacilityAsset
        {
            FacilityId = facilityId,
            Code = code,
            Name = name,
            Department = dept,
            Count = request.Count,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            Status = FacilityAssetStatus.Working,
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = updatedBy
        };

        db.FacilityAssets.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<FacilityAsset> SetStatusAsync(
        string facilityId,
        Guid assetId,
        FacilityAssetStatus status,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.FacilityAssets
            .FirstOrDefaultAsync(x => x.Id == assetId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الأصل غير موجود.");

        row.Status = status;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<FacilityAsset> CycleStatusAsync(
        string facilityId,
        Guid assetId,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.FacilityAssets
            .FirstOrDefaultAsync(x => x.Id == assetId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("الأصل غير موجود.");

        row.Status = FacilityAssetStatusLabels.Next(row.Status);
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await db.SaveChangesAsync(ct);
        return row;
    }
}
