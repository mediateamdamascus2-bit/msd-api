namespace MedicalServices.Domain.Entities;

/// <summary>Inventory line item inside a hospital warehouse section.</summary>
public class StoreItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinQuantity { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}
