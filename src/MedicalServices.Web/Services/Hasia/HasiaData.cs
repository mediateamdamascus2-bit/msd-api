namespace MedicalServices.Web.Services.Hasia;

public sealed record HasiaNavItem(string Key, string LabelAr, string Hint, string Group, bool Ready, bool MobilePrimary);

public sealed record HasiaService(string Id, string Zone, string Name, int? Capacity = null);

public sealed record HasiaFleetVehicle(
    string Plate, string Kind, string Status, string LastTrip,
    string? MissionTo = null, string? MissionReason = null, string? MissionInmate = null,
    string? Driver = null, string? Left = null, string? Eta = null, int Progress = 0, string? Stage = null);

public sealed record HasiaTripLog(string When, string Plate, string To, string Reason, string Duration);

public static class HasiaData
{
    public const string FacilityId = "hasia";
    public const string TitleAr = "سجن حسيا";
    public const string RoleAr = "المسؤول الطبي للسجن";

    public static IReadOnlyList<HasiaNavItem> Nav { get; } =
    [
        new("overview", "الرئيسية", "ملخص اليوم", "عام", true, true),
        new("structure", "البنية الطبية", "النقطة والمشفى", "عام", true, true),
        new("intake", "استقبال نزيل", "إحصائيات فقط", "المرضى", true, true),
        new("triage", "الفرز", "إحصائيات فقط", "المرضى", true, true),
        new("cases", "الحالات", "إحصائيات فقط", "المرضى", true, false),
        new("archive", "أرشيف الحالات", "الحالات المنجزة حسب اليوم", "المرضى", true, false),
        new("inmates", "سجل النزلاء", "الأضابير الطبية", "المرضى", true, false),
        new("ph-internal", "الصيدلية الداخلية", "إحصائيات فقط", "الصيدليات", true, false),
        new("ph-external", "الصيدلية الخارجية", "إحصائيات فقط", "الصيدليات", true, false),
        new("fleet", "السيارات الطبية", "متابعة فقط", "الأسطول", true, false)
    ];

    public static IReadOnlyList<string> Groups { get; } = ["عام", "المرضى", "الصيدليات", "الأسطول"];

    public static IReadOnlyList<HasiaService> Services { get; } =
    [
        new("p-er", "point", "إسعاف النقطة"),
        new("p-dent", "point", "عيادة سنية أولية"),
        new("p-int", "point", "عيادة داخلية أولية"),
        new("p-ph", "point", "صيدلية أولية (داخلية)"),
        new("h-er", "hospital", "إسعاف المشفى"),
        new("h-int", "hospital", "عيادة داخلية متقدمة"),
        new("h-surg", "hospital", "عيادة الجراحة العظمية المتقدمة"),
        new("h-or", "hospital", "غرفة العمليات"),
        new("h-icu", "hospital", "أسرّة العناية", 6),
        new("h-ward", "hospital", "أسرّة الاستشفاء", 24),
        new("h-dent", "hospital", "عيادة سنية متقدمة"),
        new("h-ph", "hospital", "صيدلية المشفى (خارجية)")
    ];

    public static HasiaService? FindService(string id) =>
        Services.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static string ZoneLabel(string zone) => zone == "hospital" ? "المشفى" : "النقطة الطبية";

    public static string PharmacyOf(string serviceId) =>
        FindService(serviceId)?.Zone == "hospital" ? "external" : "internal";

    public static IReadOnlyList<HasiaService> Transferable(string zone, string currentId) =>
        Services.Where(s => s.Zone == zone && s.Id != currentId && !s.Id.EndsWith("-ph", StringComparison.Ordinal)).ToList();

    public static IReadOnlyList<HasiaService> TriageDests =>
        Services.Where(s => !s.Id.EndsWith("-ph", StringComparison.Ordinal)).ToList();

    public static IReadOnlyList<string> PrisonNames { get; } =
    [
        "حسيا", "سجن دمشق المركزي", "مركز احتجاز عدرا", "سجن حمص",
        "سجن حلب المركزي", "سجن اللاذقية المدني", "مركز احتجاز دير الزور", "سجن حماة"
    ];

    public static IReadOnlyList<string> HistoryItems { get; } =
    [
        "عصبية", "أذنية", "هضمية", "عينية", "عظمية", "جلدية", "كلوية", "صدرية", "قلبية",
        "أورام", "سكري", "دموية", "HTN", "التهاب الكبد B", "التهاب الكبد C", "الإيدز HIV"
    ];

    public static IReadOnlyList<string> Signs { get; } =
        ["شحوب", "يرقان", "وذمة", "تجفاف", "ضخامات", "زرقة", "تبقرط الأصابع"];

    public static IReadOnlyList<string> Vitals { get; } =
        ["ضغط الدم", "الطول", "النبض", "الوزن", "الحرارة", "سكر الدم", "BMI", "الأكسجة"];

    public static IReadOnlyList<string> Exam { get; } =
    [
        "القلب والأوعية الدموية", "الجهاز التنفسي", "الجهاز العصبي", "الحالة العقلية",
        "الجهاز البولي التناسلي", "البطن والجهاز الهضمي", "الجلد", "الأطراف والجهاز الهيكلي", "الأسنان"
    ];

    public static IReadOnlyList<string> Marks { get; } =
        ["الإعاقة", "الكدمات", "ندبات أو حروق", "وشم", "جروح وسحجات"];

    public static IReadOnlyList<HasiaFleetVehicle> Fleet { get; } =
    [
        new("حمص 4-21873", "إسعاف مجهّز", "في مهمة", "الآن",
            "مشفى حمص الوطني", "تحويل لتصوير طبقي", "10587", "أبو علي", "10:05", "10:50", 62, "في الطريق إلى المشفى"),
        new("حمص 4-30912", "إسعاف", "في مهمة", "الآن",
            "مستودع حمص الطبي", "استلام أدوية للصيدلية", null, "سامر", "09:30", "11:15", 35, "في المستودع — تحميل"),
        new("حمص 4-17654", "سيارة نقل مرضى", "متوفرة", "أمس 16:40"),
        new("حمص 4-22018", "إسعاف", "صيانة", "قبل 3 أيام")
    ];

    public static IReadOnlyList<HasiaTripLog> TripLog { get; } =
    [
        new("أمس 14:10", "حمص 4-17654", "مشفى حمص الوطني", "مراجعة قلبية #10234", "2س 30د"),
        new("أمس 09:00", "حمص 4-21873", "مشفى الباسل", "إسعاف حالة جراحية #10912", "1س 45د"),
        new("قبل يومين", "حمص 4-30912", "مستودع حمص الطبي", "استلام مواد", "3س")
    ];
}
