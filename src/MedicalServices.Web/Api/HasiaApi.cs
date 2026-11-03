using System.Security.Claims;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using MedicalServices.Web.Services.Hasia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MedicalServices.Web.Api;

public static class HasiaApi
{
    public static RouteGroupBuilder MapHasiaApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme };
        var g = api.MapGroup("/facilities/hasia").RequireAuthorization(jwt);

        g.MapGet("/inmates", async (ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            var rows = await ops.ListInmatesAsync();
            return Results.Ok(rows.Select(InmateDto));
        });

        g.MapPost("/inmates", async (CreateInmateRequest dto, ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            try { return Results.Ok(InmateDto(await ops.CreateInmateAsync(dto))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        g.MapGet("/cases", async (string? status, bool? archiveOnly, ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            var rows = await ops.ListCasesAsync(status, archiveOnly == true);
            return Results.Ok(rows.Select(CaseDto));
        });

        g.MapPost("/triage", async (OpenTriageRequest dto, ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            try { return Results.Ok(CaseDto(await ops.OpenTriageCaseAsync(dto))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        g.MapPost("/cases/{caseId:guid}/step", async (Guid caseId, CaseStepRequest dto, ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            try { return Results.Ok(CaseDto(await ops.CompleteStepAsync(caseId, dto))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        g.MapGet("/pharmacy/{pharmacy}/stock", async (string pharmacy, ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            return Results.Ok(await ops.ListStockAsync(pharmacy));
        });

        g.MapGet("/pharmacy/{pharmacy}/rx", async (string pharmacy, ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            return Results.Ok(await ops.ListPrescriptionsAsync(pharmacy));
        });

        g.MapPost("/pharmacy/dispense", async (DispenseRequest dto, ClaimsPrincipal user, IHasiaOpsService ops) =>
        {
            if (!CanAccess(user)) return Results.Forbid();
            try
            {
                await ops.DispenseAsync(dto);
                return Results.Ok(new { ok = true });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        return api;
    }

    private static bool CanAccess(ClaimsPrincipal user)
    {
        if (user.IsInRole(AppRoles.Admin)) return true;
        var linked = user.FindFirst(AppClaimTypes.FacilityId)?.Value;
        return string.Equals(linked, HasiaData.FacilityId, StringComparison.OrdinalIgnoreCase);
    }

    private static object InmateDto(MedicalServices.Domain.Entities.PrisonInmate i) => new
    {
        i.Id,
        i.FacilityId,
        i.InmateNo,
        i.FileNo,
        i.Nationality,
        i.BirthPlace,
        i.BirthDate,
        i.BloodType,
        i.Sex,
        i.Marital,
        i.Place,
        i.EntryDate,
        i.ChronicCsv,
        i.Allergies,
        i.RecordJson,
        i.DiagnosesJson,
        i.CreatedAt,
        stays = i.Stays.OrderBy(s => s.SortOrder).Select(s => new { s.Id, s.SortOrder, s.Place, s.Entry, s.Exit })
    };

    private static object CaseDto(MedicalServices.Domain.Entities.PrisonCase c) => new
    {
        c.Id,
        c.FacilityId,
        c.CaseCode,
        c.InmateNo,
        c.Triager,
        c.Complaint,
        c.AtServiceId,
        c.Status,
        c.HoldReason,
        c.OpenedTime,
        c.Day,
        c.CreatedAt,
        steps = c.Steps.OrderBy(s => s.SortOrder).Select(s => new
        {
            s.Id, s.SortOrder, s.Time, s.AtServiceId, s.By, s.Diagnosis, s.Procedures, s.Meds, s.Note
        })
    };
}
