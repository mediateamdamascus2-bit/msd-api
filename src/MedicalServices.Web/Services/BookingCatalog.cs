namespace MedicalServices.Web.Services;

public static class BookingCatalog
{
    public const string DefaultFacilityId = "harasta";
    public const int DailyCap = 20;

    public const string EntityInterior = "وزارة الداخلية";
    public const string EntityDefense = "وزارة الدفاع";
    public const string EntityDetained = "موقوف";
    public const string EntityCivilian = "مدني";
    public const string EntityOther = "أخرى";

    public static IReadOnlyList<string> Slots { get; } =
    [
        "08:00", "08:30", "09:00", "09:30", "10:00", "10:30",
        "11:00", "11:30", "12:00", "12:30", "13:00", "13:30"
    ];

    public static int PerSlot => (int)Math.Ceiling(DailyCap / (double)Slots.Count);

    public static IReadOnlyList<string> Entities { get; } =
    [
        EntityInterior, EntityDefense, EntityDetained, EntityCivilian, EntityOther
    ];

    public static IReadOnlyList<string> BookableClinics { get; } =
    [
        "الداخلية", "القلبية", "الأطفال", "حديثي الولادة", "النسائية والتوليد", "الجلدية",
        "الجراحة العامة", "الجراحة العظمية", "العينية", "الأذنية", "البولية", "العصبية", "السنية",
        "الكلية وغسيل الكلى", "العلاج الفيزيائي"
    ];

    public static IReadOnlyList<string> InteriorAdmins { get; } =
    [
        "إدارة الخدمات الطبية", "قيادة شرطة دمشق", "قيادة شرطة ريف دمشق",
        "إدارة السجون", "إدارة المرور", "إدارة الهجرة والجوازات", "الأمن الجنائي"
    ];

    public static IReadOnlyList<string> Governorates { get; } =
    [
        "دمشق", "ريف دمشق", "حلب", "حمص", "حماة", "اللاذقية", "طرطوس",
        "إدلب", "درعا", "السويداء", "القنيطرة", "دير الزور", "الرقة", "الحسكة"
    ];

    public static IReadOnlyList<string> SpecialTypes { get; } =
    [
        "ابن شهيد", "بدون هوية", "جريح", "ذوو الاحتياجات الخاصة", "أخرى"
    ];

    /// <summary>Defense and civilian bookings have no affiliated unit.</summary>
    public static bool RequiresAdminUnit(string entity) =>
        !string.Equals(entity, EntityDefense, StringComparison.Ordinal)
        && !string.Equals(entity, EntityCivilian, StringComparison.Ordinal);

    public static string AdminFieldLabel(string entity) => entity switch
    {
        EntityInterior => "الإدارة (وزارة الداخلية)",
        EntityDetained => "مكان التوقيف",
        _ => "اسم الإدارة / الجهة"
    };

    public static string AdminFieldPlaceholder(string entity) => entity switch
    {
        EntityDetained => "أي مكان يُحتجز فيه المراجع",
        _ => "اسم الإدارة"
    };

    public static string MissingAdminMessage(string entity) => entity switch
    {
        EntityInterior => "اختر الإدارة التابعة لوزارة الداخلية.",
        EntityDetained => "أدخل مكان التوقيف.",
        _ => "أدخل اسم الإدارة / الجهة."
    };
}
