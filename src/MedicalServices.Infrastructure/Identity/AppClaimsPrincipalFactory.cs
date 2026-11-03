using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MedicalServices.Infrastructure.Identity;

public sealed class AppClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimTypes.GivenName, user.DisplayNameAr));

        if (!string.IsNullOrWhiteSpace(user.FacilityId))
            identity.AddClaim(new Claim(AppClaimTypes.FacilityId, user.FacilityId));

        if (!string.IsNullOrWhiteSpace(user.HomePath))
            identity.AddClaim(new Claim(AppClaimTypes.HomePath, user.HomePath));

        return identity;
    }
}
