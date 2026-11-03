using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;

namespace MedicalServices.Web.Services;

public interface IFleetService
{
    Task<IReadOnlyList<FleetVehicle>> ListAsync(string facilityId, CancellationToken ct = default);

    Task<FleetVehicle> AddAsync(string facilityId, AddFleetVehicleRequest request, string updatedBy, CancellationToken ct = default);

    Task<FleetVehicle> SetStatusAsync(
        string facilityId,
        Guid vehicleId,
        FleetVehicleStatus status,
        string? missionNote,
        string updatedBy,
        CancellationToken ct = default);
}

public sealed record AddFleetVehicleRequest(
    string Code,
    string Plate,
    string VehicleType,
    string? Equipment,
    string? DriverName,
    int OdometerKm,
    DateOnly? LastServiceDate);
