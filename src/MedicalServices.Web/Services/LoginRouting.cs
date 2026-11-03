using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;

namespace MedicalServices.Web.Services;

public static class LoginRouting
{
    public static string ResolveHomePath(ApplicationUser user, IEnumerable<string> roles)
    {
        if (!string.IsNullOrWhiteSpace(user.HomePath) && user.HomePath.StartsWith('/'))
            return user.HomePath.Trim();

        if (roles.Contains(AppRoles.Admin))
            return "/admin";

        if (roles.Contains(AppRoles.BookingClerk))
            return "/stations/harasta/booking";

        if (!string.IsNullOrWhiteSpace(user.FacilityId))
            return $"/f/{user.FacilityId}";

        return "/login";
    }

    /// <summary>
    /// Encode a relative path for HTTP Location headers (Kestrel rejects non-ASCII).
    /// Keeps '/' separators; encodes each segment (Arabic clinic/dept names).
    /// </summary>
    public static string ToLocationHeader(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/login";

        var trimmed = path.Trim();
        if (!trimmed.StartsWith('/'))
            trimmed = "/" + trimmed;

        var segments = trimmed.Split('/');
        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].Length == 0)
                continue;
            // Avoid double-encoding if already escaped.
            var raw = Uri.UnescapeDataString(segments[i]);
            segments[i] = Uri.EscapeDataString(raw);
        }

        return string.Join('/', segments);
    }

    /// <summary>Station-only accounts may stay on their HomePath (and logout/login).</summary>
    public static bool IsAllowedPath(string? homePath, string requestPath, IEnumerable<string> roles)
    {
        var roleSet = roles as ISet<string> ?? roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roleSet.Contains(AppRoles.Admin))
            return true;

        // Facility main users: facility UI only (not stations, not admin console).
        if (roleSet.Contains(AppRoles.FacilityUser) && !roleSet.Contains(AppRoles.StationUser))
        {
            if (requestPath.StartsWith("/stations/", StringComparison.OrdinalIgnoreCase))
                return false;
            if (requestPath.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        // Station / booking clerks: locked to HomePath (+ auth pages).
        if (roleSet.Contains(AppRoles.StationUser) || roleSet.Contains(AppRoles.BookingClerk))
        {
            if (string.IsNullOrWhiteSpace(homePath))
                return false;
            if (requestPath.StartsWith("/login", StringComparison.OrdinalIgnoreCase)
                || requestPath.StartsWith("/auth/", StringComparison.OrdinalIgnoreCase))
                return true;

            // Shared TV call board — any Harasta station may open it.
            if (requestPath.StartsWith("/stations/harasta/display", StringComparison.OrdinalIgnoreCase))
                return true;

            var home = Uri.UnescapeDataString(homePath).TrimEnd('/');
            var path = Uri.UnescapeDataString(requestPath).TrimEnd('/');
            return path.Equals(home, StringComparison.OrdinalIgnoreCase)
                   || path.StartsWith(home + "/", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }
}
