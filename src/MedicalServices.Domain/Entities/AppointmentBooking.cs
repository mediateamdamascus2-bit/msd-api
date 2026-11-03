using MedicalServices.Domain.Enums;

namespace MedicalServices.Domain.Entities;

/// <summary>Appointment booked by an external admin/entity for a hospital clinic.</summary>
public class AppointmentBooking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Entity { get; set; } = "وزارة الداخلية";
    public string AdminUnit { get; set; } = string.Empty;
    public string Clinic { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public string Slot { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public int? Age { get; set; }
    public string Governorate { get; set; } = "دمشق";
    public string? Residence { get; set; }
    public string Category { get; set; } = "عامة";
    public string? SpecialType { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Reserved;
    public string BookedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
