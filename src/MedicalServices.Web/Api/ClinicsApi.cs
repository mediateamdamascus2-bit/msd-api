using System.Security.Claims;
using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using MedicalServices.Infrastructure.Persistence;
using MedicalServices.Web.Services;
using MedicalServices.Web.Services.Harasta;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Api;

public static class ClinicsApi
{
    public static RouteGroupBuilder MapClinicsApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        };

        api.MapGet("/facilities/{facilityId}/clinics/counts", async (
            string facilityId,
            ClaimsPrincipal user,
            IClinicService clinics) =>
        {
            if (!CanAccess(user, facilityId))
                return ApiForbidden();

            // Clinic station accounts only need their own queue — not the hub counts grid.
            var home = user.FindFirst(AppClaimTypes.HomePath)?.Value;
            if (HarastaStationAccounts.ClinicFromHomePath(home) is not null
                && !user.IsInRole(AppRoles.Admin)
                && !user.IsInRole(AppRoles.FacilityUser))
                return ApiForbidden();

            var counts = await clinics.GetQueueCountsAsync(facilityId);
            return Results.Ok(counts.Select(kv => new { clinic = kv.Key, waiting = kv.Value }));
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{facilityId}/clinics/{clinic}/queue", async (
            string facilityId,
            string clinic,
            ClaimsPrincipal user,
            IClinicService clinics) =>
        {
            clinic = Uri.UnescapeDataString(clinic);
            if (!CanAccessClinic(user, facilityId, clinic))
                return ApiForbidden();

            var items = await clinics.GetQueueAsync(facilityId, clinic);
            return Results.Ok(items.Select(ToDto));
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/clinics/cases/{caseId:guid}/call", async (
            string facilityId,
            Guid caseId,
            ClaimsPrincipal user,
            IClinicService clinics,
            IDbContextFactory<AppDbContext> dbFactory) =>
        {
            if (!await CanAccessCaseClinicAsync(user, facilityId, caseId, dbFactory))
                return ApiForbidden();
            try
            {
                var row = await clinics.CallPatientAsync(
                    facilityId, caseId, user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/clinics/cases/{caseId:guid}/complete", async (
            string facilityId,
            Guid caseId,
            ClinicActionDto dto,
            ClaimsPrincipal user,
            IClinicService clinics,
            IDbContextFactory<AppDbContext> dbFactory) =>
        {
            if (!await CanAccessCaseClinicAsync(user, facilityId, caseId, dbFactory))
                return ApiForbidden();
            try
            {
                var row = await clinics.CompleteVisitAsync(
                    facilityId, caseId,
                    new ClinicCompleteRequest(dto.Doctor, dto.Note, dto.Note),
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/clinics/cases/{caseId:guid}/return", async (
            string facilityId,
            Guid caseId,
            ClinicReturnDto dto,
            ClaimsPrincipal user,
            IClinicService clinics,
            IDbContextFactory<AppDbContext> dbFactory) =>
        {
            if (!await CanAccessCaseClinicAsync(user, facilityId, caseId, dbFactory))
                return ApiForbidden();
            try
            {
                var row = await clinics.ReturnToReceptionAsync(
                    facilityId, caseId,
                    new ClinicReturnRequest(dto.Doctor, dto.Reason ?? string.Empty),
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/clinics/cases/{caseId:guid}/transfer", async (
            string facilityId,
            Guid caseId,
            ClinicTransferDto dto,
            ClaimsPrincipal user,
            IClinicService clinics,
            IDbContextFactory<AppDbContext> dbFactory) =>
        {
            if (!await CanAccessCaseClinicAsync(user, facilityId, caseId, dbFactory))
                return ApiForbidden();
            try
            {
                var row = await clinics.TransferClinicAsync(
                    facilityId, caseId, dto.ToClinic, dto.Doctor,
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/clinics/cases/{caseId:guid}/refer-department", async (
            string facilityId,
            Guid caseId,
            ClinicReferDto dto,
            ClaimsPrincipal user,
            IClinicService clinics,
            IDbContextFactory<AppDbContext> dbFactory) =>
        {
            if (!await CanAccessCaseClinicAsync(user, facilityId, caseId, dbFactory))
                return ApiForbidden();
            try
            {
                var row = await clinics.ReferToDepartmentAsync(
                    facilityId, caseId, dto.Department, dto.Doctor, dto.Note,
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        return api;
    }

    private static IResult ApiForbidden() =>
        Results.Json(new { error = "لا صلاحية لهذه العيادة" }, statusCode: StatusCodes.Status403Forbidden);

    private static bool CanAccess(ClaimsPrincipal user, string facilityId)
    {
        if (user.IsInRole(AppRoles.Admin))
            return string.Equals(facilityId, HarastaData.FacilityId, StringComparison.OrdinalIgnoreCase);

        var linked = user.FindFirst(AppClaimTypes.FacilityId)?.Value;
        return string.Equals(linked, facilityId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(facilityId, HarastaData.FacilityId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanAccessClinic(ClaimsPrincipal user, string facilityId, string clinic)
    {
        if (!CanAccess(user, facilityId))
            return false;

        if (user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.FacilityUser))
            return true;

        // Prefer HomePath so JWT role-claim mapping cannot bypass clinic isolation.
        var home = user.FindFirst(AppClaimTypes.HomePath)?.Value;
        var assigned = HarastaStationAccounts.ClinicFromHomePath(home);
        if (assigned is not null)
            return string.Equals(assigned, clinic, StringComparison.Ordinal);

        if (user.IsInRole(AppRoles.StationUser) || user.IsInRole(AppRoles.BookingClerk))
            return false;

        return true;
    }

    private static async Task<bool> CanAccessCaseClinicAsync(
        ClaimsPrincipal user,
        string facilityId,
        Guid caseId,
        IDbContextFactory<AppDbContext> dbFactory,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.PatientCases.AsNoTracking()
            .Where(x => x.Id == caseId && x.FacilityId == facilityId)
            .Select(x => new { x.CurrentClinic, x.BookedClinic })
            .FirstOrDefaultAsync(ct);
        if (row is null)
            return false;

        var clinic = row.CurrentClinic ?? row.BookedClinic ?? string.Empty;
        return CanAccessClinic(user, facilityId, clinic);
    }

    private static object ToDto(PatientCase row) => new
    {
        row.Id,
        row.CaseNumber,
        row.PatientName,
        row.NationalId,
        row.Age,
        row.BookedClinic,
        row.CurrentClinic,
        callCount = row.CallCount,
        status = row.Status.ToString(),
        statusAr = PatientCaseStatusLabels.ToArabic(row.Status),
        updatedAt = row.UpdatedAt
    };

    public sealed record ClinicActionDto(string Doctor, string? Note);
    public sealed record ClinicReturnDto(string Doctor, string? Reason);
    public sealed record ClinicTransferDto(string Doctor, string ToClinic);
    public sealed record ClinicReferDto(string Doctor, string Department, string? Note);
}
