using System.Security.Claims;
using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MedicalServices.Web.Api;

public static class ReceptionApi
{
    public static RouteGroupBuilder MapReceptionApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        };

        api.MapGet("/facilities/{facilityId}/reception/queue", async (
            string facilityId,
            ClaimsPrincipal user,
            IReceptionService reception) =>
        {
            if (!StationAccess.CanReadReception(user, facilityId))
                return Results.Forbid();

            var items = await reception.GetQueueAsync(facilityId);
            return Results.Ok(items.Select(ToDto));
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/reception/cases", async (
            string facilityId,
            OpenCaseDto dto,
            ClaimsPrincipal user,
            IReceptionService reception) =>
        {
            if (!StationAccess.CanWriteReception(user, facilityId))
                return Results.Forbid();

            try
            {
                var row = await reception.OpenAsync(
                    facilityId,
                    new OpenPatientCaseRequest(
                        dto.EntryType,
                        dto.Category,
                        dto.SpecialType,
                        dto.SpecialNote,
                        dto.NationalId,
                        dto.PatientName,
                        dto.Age,
                        dto.Residence,
                        dto.Governorate,
                        dto.AdminUnit,
                        dto.BookedClinic),
                    user.Identity?.Name ?? "unknown");

                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/reception/cases/{caseId:guid}/send-to-clinic", async (
            string facilityId,
            Guid caseId,
            SendToClinicDto? dto,
            ClaimsPrincipal user,
            IReceptionService reception) =>
        {
            if (!StationAccess.CanWriteReception(user, facilityId))
                return Results.Forbid();

            try
            {
                var row = await reception.SendToClinicAsync(
                    facilityId,
                    caseId,
                    dto?.Clinic,
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPut("/facilities/{facilityId}/reception/cases/{caseId:guid}", async (
            string facilityId,
            Guid caseId,
            OpenCaseDto dto,
            ClaimsPrincipal user,
            IReceptionService reception) =>
        {
            if (!StationAccess.CanWriteReception(user, facilityId))
                return Results.Forbid();

            try
            {
                var row = await reception.UpdateAtReceptionAsync(
                    facilityId,
                    caseId,
                    new UpdatePatientCaseRequest(
                        dto.Category,
                        dto.SpecialType,
                        dto.SpecialNote,
                        dto.NationalId,
                        dto.PatientName,
                        dto.Age,
                        dto.Residence,
                        dto.Governorate,
                        dto.AdminUnit,
                        dto.BookedClinic),
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapGet("/facilities/{facilityId}/reception/pending-bookings", async (
            string facilityId,
            ClaimsPrincipal user,
            IBookingService bookings) =>
        {
            if (!StationAccess.CanReadReception(user, facilityId))
                return Results.Forbid();

            var items = await bookings.ListPendingReservedAsync(facilityId);
            return Results.Ok(items.Select(BookingToDto));
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/reception/bookings/{bookingId:guid}/arrive", async (
            string facilityId,
            Guid bookingId,
            ClaimsPrincipal user,
            IReceptionService reception) =>
        {
            if (!StationAccess.CanWriteReception(user, facilityId))
                return Results.Forbid();

            try
            {
                var row = await reception.ArriveFromBookingAsync(
                    facilityId,
                    bookingId,
                    user.Identity?.Name ?? "unknown");
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        api.MapPost("/facilities/{facilityId}/reception/bookings/{bookingId:guid}/no-show", async (
            string facilityId,
            Guid bookingId,
            ClaimsPrincipal user,
            IBookingService bookings) =>
        {
            if (!StationAccess.CanWriteReception(user, facilityId))
                return Results.Forbid();

            try
            {
                var row = await bookings.MarkNoShowAsync(facilityId, bookingId);
                return Results.Ok(BookingToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(jwt);

        return api;
    }

    private static object BookingToDto(AppointmentBooking row) => new
    {
        row.Id,
        row.FacilityId,
        row.Code,
        row.Entity,
        row.AdminUnit,
        row.Clinic,
        date = row.AppointmentDate.ToString("yyyy-MM-dd"),
        row.Slot,
        row.PatientName,
        row.NationalId,
        row.Age,
        row.Governorate,
        row.Residence,
        row.Category,
        row.SpecialType,
        status = row.Status.ToString(),
        statusAr = BookingStatusLabels.ToArabic(row.Status),
        row.BookedBy,
        createdAt = row.CreatedAt
    };

    private static object ToDto(PatientCase row) => new
    {
        row.Id,
        row.FacilityId,
        row.CaseNumber,
        row.EntryType,
        row.Category,
        row.SpecialType,
        row.SpecialNote,
        row.NationalId,
        row.PatientName,
        row.Age,
        row.Residence,
        row.Governorate,
        row.AdminUnit,
        row.BookedClinic,
        row.CurrentClinic,
        status = row.Status.ToString(),
        statusAr = PatientCaseStatusLabels.ToArabic(row.Status),
        openedAt = row.OpenedAt,
        row.OpenedBy,
        updatedAt = row.UpdatedAt,
        row.UpdatedBy
    };

    public sealed record OpenCaseDto(
        string EntryType,
        string Category,
        string? SpecialType,
        string? SpecialNote,
        string? NationalId,
        string PatientName,
        int? Age,
        string? Residence,
        string Governorate,
        string? AdminUnit,
        string BookedClinic);

    public sealed record SendToClinicDto(string? Clinic);
}
