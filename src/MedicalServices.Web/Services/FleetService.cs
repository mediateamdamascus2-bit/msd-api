using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services;

public sealed class FleetService(IDbContextFactory<AppDbContext> dbFactory) : IFleetService
{
    public async Task<IReadOnlyList<FleetVehicle>> ListAsync(string facilityId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.FleetVehicles
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
    }

    public async Task<FleetVehicle> AddAsync(
        string facilityId,
        AddFleetVehicleRequest request,
        string updatedBy,
        CancellationToken ct = default)
    {
        var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant();
        var plate = (request.Plate ?? string.Empty).Trim();
        var type = (request.VehicleType ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("رمز المركبة مطلوب.");
        if (string.IsNullOrWhiteSpace(plate))
            throw new InvalidOperationException("رقم اللوحة مطلوب.");
        if (string.IsNullOrWhiteSpace(type))
            throw new InvalidOperationException("نوع المركبة مطلوب.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (await db.FleetVehicles.AnyAsync(x => x.FacilityId == facilityId && x.Code == code, ct))
            throw new InvalidOperationException("رمز المركبة مستخدم مسبقاً.");

        var row = new FleetVehicle
        {
            FacilityId = facilityId,
            Code = code,
            Plate = plate,
            VehicleType = type,
            Equipment = string.IsNullOrWhiteSpace(request.Equipment) ? null : request.Equipment.Trim(),
            DriverName = string.IsNullOrWhiteSpace(request.DriverName) ? null : request.DriverName.Trim(),
            OdometerKm = Math.Max(0, request.OdometerKm),
            LastServiceDate = request.LastServiceDate,
            Status = FleetVehicleStatus.Ready,
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = updatedBy
        };

        db.FleetVehicles.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<FleetVehicle> SetStatusAsync(
        string facilityId,
        Guid vehicleId,
        FleetVehicleStatus status,
        string? missionNote,
        string updatedBy,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.FleetVehicles
            .FirstOrDefaultAsync(x => x.Id == vehicleId && x.FacilityId == facilityId, ct)
            ?? throw new InvalidOperationException("المركبة غير موجودة.");

        if (status == FleetVehicleStatus.OnMission)
        {
            if (row.Status == FleetVehicleStatus.Maintenance)
                throw new InvalidOperationException("لا يمكن إرسال مركبة في الصيانة إلى مهمة.");
            if (string.IsNullOrWhiteSpace(missionNote))
                throw new InvalidOperationException("أدخل وصف المهمة.");
            row.MissionNote = missionNote.Trim();
        }
        else
        {
            row.MissionNote = null;
            if (status == FleetVehicleStatus.Ready && row.Status == FleetVehicleStatus.Maintenance)
                row.LastServiceDate = DateOnly.FromDateTime(DateTime.Today);
        }

        row.Status = status;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await db.SaveChangesAsync(ct);
        return row;
    }
}
