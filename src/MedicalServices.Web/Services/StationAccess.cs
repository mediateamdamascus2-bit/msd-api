using System.Security.Claims;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using MedicalServices.Web.Services.Harasta;

namespace MedicalServices.Web.Services;

/// <summary>Shared facility / station authorization for JWT APIs and UI.</summary>
public static class StationAccess
{
    public static string? FacilityId(ClaimsPrincipal user) =>
        user.FindFirst(AppClaimTypes.FacilityId)?.Value;

    public static string? HomePath(ClaimsPrincipal user) =>
        user.FindFirst(AppClaimTypes.HomePath)?.Value;

    public static bool SameFacility(ClaimsPrincipal user, string facilityId)
    {
        if (user.IsInRole(AppRoles.Admin))
            return true;

        var linked = FacilityId(user);
        return string.Equals(linked, facilityId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsReceptionHome(string? homePath)
    {
        if (string.IsNullOrWhiteSpace(homePath))
            return false;
        var path = Uri.UnescapeDataString(homePath).TrimEnd('/');
        return path.Equals("/stations/harasta/reception", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsBookingHome(string? homePath)
    {
        if (string.IsNullOrWhiteSpace(homePath))
            return false;
        var path = Uri.UnescapeDataString(homePath).TrimEnd('/');
        return path.Equals("/stations/harasta/booking", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDisplayHome(string? homePath)
    {
        if (string.IsNullOrWhiteSpace(homePath))
            return false;
        var path = Uri.UnescapeDataString(homePath).TrimEnd('/');
        return path.Equals("/stations/harasta/display", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Read reception queue / pending bookings: admin, facility manager, or reception station.</summary>
    public static bool CanReadReception(ClaimsPrincipal user, string facilityId)
    {
        if (!SameFacility(user, facilityId))
            return false;
        if (user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.FacilityUser))
            return true;
        return IsReceptionHome(HomePath(user));
    }

    /// <summary>Mutate reception cases / arrivals: admin or reception station only.</summary>
    public static bool CanWriteReception(ClaimsPrincipal user, string facilityId)
    {
        if (!SameFacility(user, facilityId))
            return false;
        if (user.IsInRole(AppRoles.Admin))
            return true;
        return IsReceptionHome(HomePath(user));
    }

    public static bool CanAccessBooking(ClaimsPrincipal user, string facilityId)
    {
        if (!SameFacility(user, facilityId))
            return false;
        if (user.IsInRole(AppRoles.Admin))
            return true;
        if (user.IsInRole(AppRoles.BookingClerk))
            return IsBookingHome(HomePath(user)) || string.IsNullOrWhiteSpace(HomePath(user));
        return false;
    }

    public static bool CanAccessHarastaFacility(ClaimsPrincipal user, string facilityId) =>
        SameFacility(user, facilityId)
        && string.Equals(facilityId, HarastaData.FacilityId, StringComparison.OrdinalIgnoreCase);

    public static string? DeptFromHomePath(string? homePath)
    {
        if (string.IsNullOrWhiteSpace(homePath))
            return null;

        const string prefix = "/stations/harasta/dept/";
        var path = Uri.UnescapeDataString(homePath.Trim()).TrimEnd('/');
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var dept = path[prefix.Length..].Trim();
        return HarastaData.Departments.Contains(dept) ? dept : null;
    }
}
