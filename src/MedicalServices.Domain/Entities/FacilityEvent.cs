namespace MedicalServices.Domain.Entities;

/// <summary>Operational event logged at a medical unit / facility.</summary>
public class FacilityEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Severity { get; set; } = "عادي";
    public string? Details { get; set; }
    public string Status { get; set; } = "مفتوح";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ClosedBy { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
