namespace MedicalServices.Domain.Entities;

/// <summary>Daily operational counts for a medical unit / facility.</summary>
public class DailyAdmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public DateOnly Day { get; set; }
    public int AdmissionsCount { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public int TransfersCount { get; set; }
    public string? Notes { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
