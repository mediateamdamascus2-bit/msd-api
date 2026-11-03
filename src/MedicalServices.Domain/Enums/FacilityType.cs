namespace MedicalServices.Domain.Enums;

public enum FacilityType
{
    Hospital = 1,
    PrisonClinic = 2,
    MedicalUnit = 3,
    MedicalCenter = 4,
    Warehouse = 5
}

public static class FacilityTypeLabels
{
    public static string ToArabic(FacilityType type) => type switch
    {
        FacilityType.Hospital => "مشفى",
        FacilityType.PrisonClinic => "عيادة سجن",
        FacilityType.MedicalUnit => "وحدة طبية",
        FacilityType.MedicalCenter => "مركز رعاية",
        FacilityType.Warehouse => "مستودع",
        _ => type.ToString()
    };
}
