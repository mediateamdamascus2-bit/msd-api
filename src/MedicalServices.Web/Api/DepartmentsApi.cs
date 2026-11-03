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

public static class DepartmentsApi
{
    public static RouteGroupBuilder MapDepartmentsApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        };

        api.MapGet("/facilities/{facilityId}/departments/counts", async (
            string facilityId,
            ClaimsPrincipal user,
            IDepartmentService departments) =>
        {
            if (!CanAccess(user, facilityId))
                return ApiForbidden();

            var counts = await departments.GetQueueCountsAsync(facilityId);
            return Results.Ok(counts.Select(kv => new { department = kv.Key, waiting = kv.Value }));
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{facilityId}/departments/live", async (
            string facilityId,
            ClaimsPrincipal user,
            IDepartmentService departments) =>
        {
            if (!CanAccess(user, facilityId))
                return ApiForbidden();

            var cards = await departments.GetLiveBoardAsync(facilityId);
            return Results.Ok(cards);
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{facilityId}/departments/{department}/queue", async (
            string facilityId,
            string department,
            ClaimsPrincipal user,
            IDepartmentService departments) =>
        {
            department = Uri.UnescapeDataString(department);
            if (!CanAccessDepartment(user, facilityId, department))
                return ApiForbidden();

            var items = await departments.GetQueueAsync(facilityId, department);
            return Results.Ok(items.Select(ToDto));
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/departments/cases/{caseId:guid}/complete", async (
            string facilityId,
            Guid caseId,
            DeptActionDto dto,
            ClaimsPrincipal user,
            IDepartmentService departments,
            IDbContextFactory<AppDbContext> dbFactory) =>
        {
            if (!await CanAccessCaseDepartmentAsync(user, facilityId, caseId, dbFactory))
                return ApiForbidden();
            try
            {
                var row = await departments.CompleteAsync(
                    facilityId, caseId, dto.Result ?? string.Empty,
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/departments/cases/{caseId:guid}/return-clinic", async (
            string facilityId,
            Guid caseId,
            DeptActionDto dto,
            ClaimsPrincipal user,
            IDepartmentService departments,
            IDbContextFactory<AppDbContext> dbFactory) =>
        {
            if (!await CanAccessCaseDepartmentAsync(user, facilityId, caseId, dbFactory))
                return ApiForbidden();
            try
            {
                var row = await departments.ReturnToClinicAsync(
                    facilityId, caseId, dto.Result ?? string.Empty,
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
        Results.Json(new { error = "لا صلاحية لهذا القسم" }, statusCode: StatusCodes.Status403Forbidden);

    private static bool CanAccess(ClaimsPrincipal user, string facilityId)
    {
        if (user.IsInRole(AppRoles.Admin))
            return string.Equals(facilityId, HarastaData.FacilityId, StringComparison.OrdinalIgnoreCase);

        var linked = user.FindFirst(AppClaimTypes.FacilityId)?.Value;
        return string.Equals(linked, facilityId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(facilityId, HarastaData.FacilityId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanAccessDepartment(ClaimsPrincipal user, string facilityId, string department)
    {
        if (!CanAccess(user, facilityId))
            return false;

        if (user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.FacilityUser))
            return true;

        var home = user.FindFirst(AppClaimTypes.HomePath)?.Value;
        var assigned = StationAccess.DeptFromHomePath(home);
        if (assigned is not null)
            return string.Equals(assigned, department, StringComparison.Ordinal);

        if (user.IsInRole(AppRoles.StationUser) || user.IsInRole(AppRoles.BookingClerk))
            return false;

        return true;
    }

    private static async Task<bool> CanAccessCaseDepartmentAsync(
        ClaimsPrincipal user,
        string facilityId,
        Guid caseId,
        IDbContextFactory<AppDbContext> dbFactory,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var dept = await db.PatientCases.AsNoTracking()
            .Where(x => x.Id == caseId && x.FacilityId == facilityId)
            .Select(x => x.CurrentDepartment)
            .FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(dept))
            return false;

        return CanAccessDepartment(user, facilityId, dept);
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
        row.CurrentDepartment,
        row.DepartmentNote,
        status = row.Status.ToString(),
        statusAr = PatientCaseStatusLabels.ToArabic(row.Status),
        updatedAt = row.UpdatedAt
    };

    public sealed record DeptActionDto(string? Result);
}
