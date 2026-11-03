using MedicalServices.Domain.Enums;

namespace MedicalServices.Domain.Entities;

/// <summary>Medical equipment / facility asset tracked at a hospital.</summary>
public class FacilityAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public FacilityAssetStatus Status { get; set; } = FacilityAssetStatus.Working;
    public string? Note { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}
