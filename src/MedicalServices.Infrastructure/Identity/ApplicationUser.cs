using Microsoft.AspNetCore.Identity;

namespace MedicalServices.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayNameAr { get; set; } = string.Empty;

    /// <summary>Linked facility for facility users; null for admins.</summary>
    public string? FacilityId { get; set; }

    /// <summary>Post-login destination (facility home or isolated station path).</summary>
    public string? HomePath { get; set; }
}
