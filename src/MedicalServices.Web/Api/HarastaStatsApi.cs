using System.Security.Claims;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using MedicalServices.Web.Services;
using MedicalServices.Web.Services.Harasta;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MedicalServices.Web.Api;

public static class HarastaStatsApi
{
    public static RouteGroupBuilder MapHarastaStatsApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        };

        api.MapGet("/facilities/{facilityId}/clinics/live", async (
            string facilityId,
            ClaimsPrincipal user,
            IHarastaHomeStatsService stats) =>
        {
            if (!CanAccess(user, facilityId))
                return Results.Json(new { error = "لا صلاحية" }, statusCode: StatusCodes.Status403Forbidden);

            var rows = await stats.GetClinicLiveAsync(facilityId);
            return Results.Ok(rows.Select(r => new
            {
                clinic = r.Clinic,
                waiting = r.Waiting,
                todayReferrals = r.TodayReferrals,
                avgWaitLabel = r.AvgWaitLabel,
                maxWaitLabel = r.MaxWaitLabel
            }));
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{facilityId}/reception/day-stats", async (
            string facilityId,
            ClaimsPrincipal user,
            IHarastaHomeStatsService stats) =>
        {
            if (!CanAccess(user, facilityId))
                return Results.Json(new { error = "لا صلاحية" }, statusCode: StatusCodes.Status403Forbidden);

            var snap = await stats.GetReceptionDayAsync(facilityId);
            return Results.Ok(new
            {
                snap.EnteredToday,
                snap.AtReceptionNow,
                snap.DirectedToday,
                snap.EmergencyToday,
                snap.BookingArrivalsToday,
                snap.ReturnedToday,
                snap.PendingBookings,
                destinations = snap.Destinations.Select(d => new
                {
                    destination = d.Destination,
                    count = d.Count,
                    percent = d.Percent
                })
            });
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{facilityId}/ops", async (
            string facilityId,
            ClaimsPrincipal user,
            IHarastaHomeStatsService stats) =>
        {
            if (!CanAccess(user, facilityId))
                return Results.Json(new { error = "لا صلاحية" }, statusCode: StatusCodes.Status403Forbidden);

            var snap = await stats.GetOpsAsync(facilityId, todayOnly: true);
            return Results.Ok(snap.Items.Select(i => new
            {
                key = i.Key,
                value = i.Value,
                detail = i.Detail,
                tone = i.Tone
            }));
        }).RequireAuthorization(jwt);

        return api;
    }

    private static bool CanAccess(ClaimsPrincipal user, string facilityId)
    {
        if (user.IsInRole(AppRoles.Admin))
            return string.Equals(facilityId, HarastaData.FacilityId, StringComparison.OrdinalIgnoreCase);

        var linked = user.FindFirst(AppClaimTypes.FacilityId)?.Value;
        return string.Equals(linked, facilityId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(facilityId, HarastaData.FacilityId, StringComparison.OrdinalIgnoreCase);
    }
}
