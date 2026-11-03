using MedicalServices.Domain.Enums;

namespace MedicalServices.Domain.Entities;

public class Facility
{
    public string Id { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public FacilityType Type { get; set; }
    public string ProvinceAr { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
