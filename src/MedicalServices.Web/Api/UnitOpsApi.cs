using System.Security.Claims;
using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using MedicalServices.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MedicalServices.Web.Api;

public static class UnitOpsApi
{
    public static RouteGroupBuilder MapUnitOpsApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        };

        api.MapGet("/facilities/{facilityId}/events", async (
            string facilityId,
            bool? openOnly,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IUnitOpsService ops) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();
            var rows = await ops.ListEventsAsync(facilityId, openOnly == true);
            return Results.Ok(rows.Select(EventDto));
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/events", async (
            string facilityId,
            CreateEventDto dto,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IUnitOpsService ops) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();
            try
            {
                var row = await ops.CreateEventAsync(
                    facilityId,
                    new CreateFacilityEventRequest(dto.Title, dto.Kind, dto.Severity, dto.Details),
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(EventDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/events/{eventId:guid}/close", async (
            string facilityId,
            Guid eventId,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IUnitOpsService ops) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();
            try
            {
                var row = await ops.CloseEventAsync(facilityId, eventId, user.Identity?.Name ?? "unknown");
                return Results.Ok(EventDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{facilityId}/needs", async (
            string facilityId,
            bool? openOnly,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IUnitOpsService ops) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();
            var rows = await ops.ListNeedsAsync(facilityId, openOnly == true);
            return Results.Ok(rows.Select(NeedDto));
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/needs", async (
            string facilityId,
            CreateNeedDto dto,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IUnitOpsService ops) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();
            try
            {
                var row = await ops.CreateNeedAsync(
                    facilityId,
                    new CreateFacilityNeedRequest(dto.Title, dto.Category, dto.Priority, dto.Quantity, dto.Notes),
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(NeedDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/needs/{needId:guid}/status", async (
            string facilityId,
            Guid needId,
            NeedStatusDto dto,
            ClaimsPrincipal user,
            IFacilityCatalog facilities,
            IUnitOpsService ops) =>
        {
            if (!await CanAccessAsync(user, facilityId, facilities))
                return Results.Forbid();
            try
            {
                var row = await ops.UpdateNeedStatusAsync(
                    facilityId, needId, dto.Status, user.Identity?.Name ?? "unknown");
                return Results.Ok(NeedDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        return api;
    }

    private static async Task<bool> CanAccessAsync(
        ClaimsPrincipal user, string facilityId, IFacilityCatalog facilities)
    {
        if (user.IsInRole(AppRoles.Admin))
            return await facilities.FindAsync(facilityId) is not null;
        var linked = user.FindFirst(AppClaimTypes.FacilityId)?.Value;
        return string.Equals(linked, facilityId, StringComparison.OrdinalIgnoreCase);
    }

    private static object EventDto(FacilityEvent row) => new
    {
        row.Id,
        row.FacilityId,
        row.Title,
        row.Kind,
        row.Severity,
        row.Details,
        row.Status,
        row.CreatedBy,
        createdAt = row.CreatedAt,
        row.ClosedBy,
        closedAt = row.ClosedAt
    };

    private static object NeedDto(FacilityNeed row) => new
    {
        row.Id,
        row.FacilityId,
        row.Title,
        row.Category,
        row.Priority,
        row.Quantity,
        row.Notes,
        row.Status,
        row.CreatedBy,
        createdAt = row.CreatedAt,
        row.UpdatedBy,
        updatedAt = row.UpdatedAt
    };

    public sealed record CreateEventDto(string Title, string Kind, string Severity, string? Details);
    public sealed record CreateNeedDto(string Title, string Category, string Priority, int Quantity, string? Notes);
    public sealed record NeedStatusDto(string Status);
}
