using System.Security.Claims;
using MedicalServices.Domain.Entities;
using MedicalServices.Domain.Enums;
using MedicalServices.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MedicalServices.Web.Api;

public static class BookingApi
{
    public static RouteGroupBuilder MapBookingApi(this RouteGroupBuilder api)
    {
        var jwt = new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Roles = $"{AppRoles.Admin},{AppRoles.BookingClerk}"
        };

        var group = api.MapGroup("/stations/{facilityId}/booking").RequireAuthorization(jwt);

        group.MapGet("/availability", async (
            string facilityId,
            DateOnly? date,
            ClaimsPrincipal user,
            IBookingService bookings) =>
        {
            if (!StationAccess.CanAccessBooking(user, facilityId))
                return Results.Forbid();

            var day = date ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var left = await bookings.GetDayAvailabilityAsync(facilityId, day);
            return Results.Ok(new
            {
                facilityId,
                date = day.ToString("yyyy-MM-dd"),
                dailyCap = BookingCatalog.DailyCap,
                slots = BookingCatalog.Slots,
                clinics = left.Select(kv => new { clinic = kv.Key, remaining = kv.Value })
            });
        });

        group.MapGet("/list", async (
            string facilityId,
            string entity,
            string? adminUnit,
            ClaimsPrincipal user,
            IBookingService bookings) =>
        {
            if (!StationAccess.CanAccessBooking(user, facilityId))
                return Results.Forbid();

            var items = await bookings.ListAsync(facilityId, entity, adminUnit ?? string.Empty);
            return Results.Ok(items.Select(ToDto));
        });

        group.MapGet("/patients/search", async (
            string facilityId,
            string? q,
            string? entity,
            string? adminUnit,
            ClaimsPrincipal user,
            IBookingService bookings) =>
        {
            if (!StationAccess.CanAccessBooking(user, facilityId))
                return Results.Forbid();

            var items = await bookings.SearchPatientsAsync(
                facilityId, q ?? string.Empty, entity, adminUnit);
            return Results.Ok(items.Select(p => new
            {
                p.PatientName,
                p.NationalId,
                p.Age,
                p.Governorate,
                p.Residence,
                p.Category,
                p.SpecialType,
                p.LastClinic,
                lastAppointmentDate = p.LastAppointmentDate?.ToString("yyyy-MM-dd"),
                p.BookingCount
            }));
        });

        group.MapPost("/", async (
            string facilityId,
            CreateBookingDto dto,
            ClaimsPrincipal user,
            IBookingService bookings) =>
        {
            if (!StationAccess.CanAccessBooking(user, facilityId))
                return Results.Forbid();

            try
            {
                if (!DateOnly.TryParse(dto.AppointmentDate, out var day))
                    return Results.BadRequest(new { error = "تاريخ غير صالح." });

                var created = await bookings.CreateAsync(
                    facilityId,
                    new CreateBookingsRequest(
                        dto.Entity,
                        dto.AdminUnit ?? string.Empty,
                        dto.Clinic,
                        day,
                        dto.PreferredSlot,
                        dto.BookedBy,
                        dto.People.Select(p => new BookingPersonRequest(
                            p.PatientName,
                            p.NationalId,
                            p.Age,
                            p.Governorate,
                            p.Residence,
                            p.Category,
                            p.SpecialType)).ToList()));

                return Results.Ok(created.Select(ToDto));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/{bookingId:guid}/cancel", async (
            string facilityId,
            Guid bookingId,
            ClaimsPrincipal user,
            IBookingService bookings) =>
        {
            if (!StationAccess.CanAccessBooking(user, facilityId))
                return Results.Forbid();

            try
            {
                var row = await bookings.CancelAsync(facilityId, bookingId);
                return Results.Ok(ToDto(row));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        return api;
    }

    private static object ToDto(AppointmentBooking row) => new
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

    public sealed record CreateBookingDto(
        string Entity,
        string? AdminUnit,
        string Clinic,
        string AppointmentDate,
        string PreferredSlot,
        string BookedBy,
        List<BookingPersonDto> People);

    public sealed record BookingPersonDto(
        string PatientName,
        string? NationalId,
        int? Age,
        string Governorate,
        string? Residence,
        string Category,
        string? SpecialType);
}
