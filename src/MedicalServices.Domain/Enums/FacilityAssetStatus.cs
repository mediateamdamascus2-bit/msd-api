namespace MedicalServices.Domain.Enums;

public enum FacilityAssetStatus
{
    Working = 0,
    NeedsMaintenance = 1,
    OutOfService = 2
}

public static class FacilityAssetStatusLabels
{
    public static string ToArabic(FacilityAssetStatus status) => status switch
    {
        FacilityAssetStatus.Working => "يعمل",
        FacilityAssetStatus.NeedsMaintenance => "يحتاج صيانة",
        FacilityAssetStatus.OutOfService => "معطل",
        _ => status.ToString()
    };

    public static FacilityAssetStatus Next(FacilityAssetStatus status) => status switch
    {
        FacilityAssetStatus.Working => FacilityAssetStatus.NeedsMaintenance,
        FacilityAssetStatus.NeedsMaintenance => FacilityAssetStatus.OutOfService,
        _ => FacilityAssetStatus.Working
    };
}
