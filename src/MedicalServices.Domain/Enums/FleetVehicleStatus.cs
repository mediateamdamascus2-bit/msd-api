namespace MedicalServices.Domain.Enums;

public enum FleetVehicleStatus
{
    Ready = 0,
    OnMission = 1,
    Maintenance = 2
}

public static class FleetVehicleStatusLabels
{
    public static string ToArabic(FleetVehicleStatus status) => status switch
    {
        FleetVehicleStatus.Ready => "جاهزة",
        FleetVehicleStatus.OnMission => "في مهمة",
        FleetVehicleStatus.Maintenance => "صيانة",
        _ => status.ToString()
    };
}
