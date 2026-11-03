using System.Security.Claims;
using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using MedicalServices.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MedicalServices.Web.Api;

public static class AdmissionsApi
{
    public static RouteGroupBuilder MapAdmissionsApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        };

        api.MapGet("/facilities/{facilityId}/admissions/today", async (
            string facilityId,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IAdmissionsService admissions) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();

            var row = await admissions.GetOrCreateTodayAsync(facilityId);
            return Results.Ok(ToDto(row));
        }).RequireAuthorization(jwt);

        api.MapPut("/facilities/{facilityId}/admissions/today", async (
            string facilityId,
            AdmissionsUpdateDto dto,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IAdmissionsService admissions) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();

            var updatedBy = user.Identity?.Name ?? "unknown";
            var row = await admissions.UpsertTodayAsync(
                facilityId,
                new AdmissionsUpdate(
                    dto.AdmissionsCount,
                    dto.OccupiedBeds,
                    dto.AvailableBeds,
                    dto.TransfersCount,
                    dto.Notes),
                updatedBy);

            return Results.Ok(ToDto(row));
        }).RequireAuthorization(jwt);

        return api;
    }

    private static async Task<bool> CanAccessAsync(
        ClaimsPrincipal user,
        string facilityId,
        IFacilityCatalog facilities)
    {
        if (user.IsInRole(AppRoles.Admin))
            return await facilities.FindAsync(facilityId) is not null;

        var linked = user.FindFirst(AppClaimTypes.FacilityId)?.Value;
        return string.Equals(linked, facilityId, StringComparison.OrdinalIgnoreCase);
    }

    private static object ToDto(DailyAdmission row) => new
    {
        row.Id,
        row.FacilityId,
        day = row.Day.ToString("yyyy-MM-dd"),
        row.AdmissionsCount,
        row.OccupiedBeds,
        row.AvailableBeds,
        row.TransfersCount,
        row.Notes,
        row.UpdatedBy,
        updatedAt = row.UpdatedAt
    };

    public sealed record AdmissionsUpdateDto(
        int AdmissionsCount,
        int OccupiedBeds,
        int AvailableBeds,
        int TransfersCount,
        string? Notes);
}
