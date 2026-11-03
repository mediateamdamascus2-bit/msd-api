using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public sealed record ReceptionAlert(
    string FacilityId,
    string Title,
    string Message,
    string? BookingCode,
    DateOnly AppointmentDate,
    string Clinic,
    DateTimeOffset At);

public interface IReceptionNotifier
{
    event Action<ReceptionAlert>? AlertReceived;

    void Publish(ReceptionAlert alert);

    void NotifyBookingCreated(string facilityId, IReadOnlyList<AppointmentBooking> bookings);

    void NotifyReturnedToReception(string facilityId, string patientName, string caseNumber, string fromClinic, string reason);
}

public sealed class ReceptionNotifier : IReceptionNotifier
{
    public event Action<ReceptionAlert>? AlertReceived;

    public void Publish(ReceptionAlert alert) => AlertReceived?.Invoke(alert);

    public void NotifyBookingCreated(string facilityId, IReadOnlyList<AppointmentBooking> bookings)
    {
        foreach (var b in bookings)
        {
            var side = string.IsNullOrWhiteSpace(b.AdminUnit)
                ? b.Entity
                : $"{b.Entity} — {b.AdminUnit}";

            Publish(new ReceptionAlert(
                facilityId,
                Title: "حجز جديد من الإدارات",
                Message: $"{b.PatientName} · رمز {b.Code} · عيادة {b.Clinic} · {b.AppointmentDate:yyyy-MM-dd} {b.Slot} · {side}",
                BookingCode: b.Code,
                AppointmentDate: b.AppointmentDate,
                Clinic: b.Clinic,
                At: DateTimeOffset.Now));
        }
    }

    public void NotifyReturnedToReception(
        string facilityId, string patientName, string caseNumber, string fromClinic, string reason)
    {
        Publish(new ReceptionAlert(
            facilityId,
            Title: "إعادة إلى الاستقبال",
            Message: $"{patientName} ({caseNumber}) أُعيد من عيادة {fromClinic}: {reason}",
            BookingCode: caseNumber,
            AppointmentDate: DateOnly.FromDateTime(DateTime.Today),
            Clinic: fromClinic,
            At: DateTimeOffset.Now));
    }
}
