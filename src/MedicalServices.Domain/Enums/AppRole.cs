namespace MedicalServices.Domain.Enums;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string FacilityUser = "FacilityUser";
    public const string BookingClerk = "BookingClerk";
    /// <summary>Secondary station account — lands on HomePath only.</summary>
    public const string StationUser = "StationUser";
}

public enum AppRole
{
    Admin = 1,
    FacilityUser = 2,
    BookingClerk = 3,
    StationUser = 4
}
