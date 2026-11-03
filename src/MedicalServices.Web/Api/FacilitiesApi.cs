using System.Security.Claims;
using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Web.Services;
using MedicalServices.Web.Services.Facilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MedicalServices.Web.Api;

public static class FacilitiesApi
{
    public static RouteGroupBuilder MapFacilitiesApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        };

        api.MapGet("/facilities", async (ClaimsPrincipal user, IFacilityCatalog catalog) =>
        {
            var all = await catalog.GetAllAsync();
            IEnumerable<Facility> items = all;
            if (!user.IsInRole(AppRoles.Admin))
            {
                var linked = StationAccess.FacilityId(user);
                items = all.Where(f => string.Equals(f.Id, linked, StringComparison.OrdinalIgnoreCase));
            }

            return Results.Ok(items.Select(f => new
            {
                f.Id,
                f.NameAr,
                type = f.Type.ToString(),
                typeAr = FacilityTypeLabels.ToArabic(f.Type),
                f.ProvinceAr,
                f.IsActive,
                path = f.Id == "harasta" ? "/f/harasta" : $"/f/{f.Id}"
            }));
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{id}", async (string id, ClaimsPrincipal user, IFacilityCatalog catalog) =>
        {
            if (!user.IsInRole(AppRoles.Admin)
                && !string.Equals(StationAccess.FacilityId(user), id, StringComparison.OrdinalIgnoreCase))
                return Results.Forbid();

            var f = await catalog.FindAsync(id);
            if (f is null)
                return Results.NotFound();

            var profile = FacilityTypeProfiles.For(f.Type);
            return Results.Ok(new
            {
                f.Id,
                f.NameAr,
                type = f.Type.ToString(),
                typeAr = FacilityTypeLabels.ToArabic(f.Type),
                f.ProvinceAr,
                f.IsActive,
                profile = new
                {
                    profile.Eyebrow,
                    profile.OverviewLead,
                    kpis = profile.Kpis,
                    nav = profile.Nav.Select(n => new { n.Key, n.LabelAr, n.Ready })
                }
            });
        }).RequireAuthorization(jwt);

        return api;
    }
}
