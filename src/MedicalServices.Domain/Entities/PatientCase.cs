using MedicalServices.Domain.Enums;

namespace MedicalServices.Domain.Entities;

/// <summary>Open patient dossier / reception case for a hospital facility.</summary>
public class PatientCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public string EntryType { get; set; } = "حجز مسبق";
    public string Category { get; set; } = "عامة";
    public string? SpecialType { get; set; }
    public string? SpecialNote { get; set; }
    public string? NationalId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Residence { get; set; }
    public string Governorate { get; set; } = "ريف دمشق";
    public string? AdminUnit { get; set; }
    public string BookedClinic { get; set; } = string.Empty;
    public string? CurrentClinic { get; set; }
    public string? CurrentDepartment { get; set; }
    public string? DepartmentNote { get; set; }
    public PatientCaseStatus Status { get; set; } = PatientCaseStatus.AtReception;
    public DateTimeOffset? CalledAt { get; set; }
    public int CallCount { get; set; }
    public DateTimeOffset OpenedAt { get; set; } = DateTimeOffset.UtcNow;
    public string OpenedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}
