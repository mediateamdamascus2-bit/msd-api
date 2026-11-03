namespace MedicalServices.Domain.Entities;

/// <summary>Supply / staffing / support need request for a medical unit.</summary>
public class FacilityNeed
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "عادي";
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
    public string Status { get; set; } = "مفتوح";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
