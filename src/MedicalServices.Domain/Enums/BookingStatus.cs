namespace MedicalServices.Domain.Enums;

public enum BookingStatus
{
    Reserved = 0,
    Arrived = 1,
    NoShow = 2,
    Cancelled = 3
}

public static class BookingStatusLabels
{
    public static string ToArabic(BookingStatus status) => status switch
    {
        BookingStatus.Reserved => "محجوز",
        BookingStatus.Arrived => "حضر",
        BookingStatus.NoShow => "لم يحضر",
        BookingStatus.Cancelled => "ملغى",
        _ => status.ToString()
    };
}
