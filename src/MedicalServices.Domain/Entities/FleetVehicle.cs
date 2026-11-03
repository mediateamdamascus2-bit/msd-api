using MedicalServices.Domain.Enums;

namespace MedicalServices.Domain.Entities;

public class FleetVehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string? Equipment { get; set; }
    public string? DriverName { get; set; }
    public FleetVehicleStatus Status { get; set; } = FleetVehicleStatus.Ready;
    public int OdometerKm { get; set; }
    public DateOnly? LastServiceDate { get; set; }
    public string? MissionNote { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}
