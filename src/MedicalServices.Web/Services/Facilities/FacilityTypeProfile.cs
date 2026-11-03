using MedicalServices.Domain.Enums;

namespace MedicalServices.Web.Services.Facilities;

public sealed record FacilityNavItem(string Key, string LabelAr, bool Ready, bool MobilePrimary);

public sealed record FacilityKpi(string Label, string Value);

public sealed record FacilityAction(string Label, string Hint, bool Ready);

public sealed class FacilityTypeProfile
{
    public required string Eyebrow { get; init; }
    public required IReadOnlyList<FacilityNavItem> Nav { get; init; }
    public required IReadOnlyList<FacilityKpi> Kpis { get; init; }
    public required IReadOnlyList<FacilityAction> Actions { get; init; }
    public required string OverviewLead { get; init; }
}

public static class FacilityTypeProfiles
{
    public static FacilityTypeProfile For(FacilityType type) => type switch
    {
        FacilityType.PrisonClinic => Prison,
        FacilityType.MedicalUnit => Unit,
        FacilityType.MedicalCenter => Center,
        FacilityType.Warehouse => Warehouse,
        FacilityType.Hospital => Hospital,
        _ => Hospital
    };

    private static readonly FacilityTypeProfile Prison = new()
    {
        Eyebrow = "PRISON CLINIC",
        OverviewLead = "واجهة صحة السجون: نقطة طبية داخلية ومشفى ضمن الحرم — متجاوبة للجوال.",
        Nav =
        [
            new("overview", "الرئيسية", true, true),
            new("services", "الخدمات", true, true),
            new("cases", "الحالات", false, true),
            new("pharmacy", "الصيدلية", false, true),
            new("files", "الملفات", false, false)
        ],
        Kpis =
        [
            new("خدمات النقطة", "4"),
            new("خدمات المشفى", "8"),
            new("أسرّة عناية", "6"),
            new("أسرّة استشفاء", "24")
        ],
        Actions =
        [
            new("الخدمات الطبية", "النقطة والمشفى داخل الحرم", true),
            new("الحالات الجارية", "قريباً", false),
            new("الصيدلية", "قريباً", false),
            new("ملفات النزلاء", "قريباً", false)
        ]
    };

    private static readonly FacilityTypeProfile Unit = new()
    {
        Eyebrow = "MEDICAL UNIT",
        OverviewLead = "واجهة الوحدة/النقطة الميدانية: قبولات، أحداث، واحتياجات — مناسبة للجوال أولاً.",
        Nav =
        [
            new("overview", "الرئيسية", true, true),
            new("admissions", "القبولات", true, true),
            new("events", "الأحداث", true, true),
            new("needs", "الاحتياجات", true, true),
            new("ambulance", "إسعاف", false, false)
        ],
        Kpis =
        [
            new("قبولات اليوم", "67"),
            new("أسرّة مشغولة", "15"),
            new("أسرّة متاحة", "12"),
            new("أحداث مفتوحة", "3")
        ],
        Actions =
        [
            new("تحديث القبولات", "إدخال أرقام اليوم", true),
            new("تسجيل حدث", "جاهز", true),
            new("طلب احتياج", "جاهز", true),
            new("طلب إسعاف", "قريباً", false)
        ]
    };

    private static readonly FacilityTypeProfile Center = new()
    {
        Eyebrow = "CARE CENTER",
        OverviewLead = "واجهة مركز الرعاية: عيادات ومراجعات يومية.",
        Nav =
        [
            new("overview", "الرئيسية", true, true),
            new("clinics", "العيادات", true, true),
            new("visits", "المراجعات", false, true),
            new("staff", "الكوادر", false, true),
            new("reports", "تقارير", false, false)
        ],
        Kpis =
        [
            new("عيادات عاملة", "8"),
            new("مراجعو اليوم", "398"),
            new("كوادر طبية", "85"),
            new("يحتاج دعمًا", "1")
        ],
        Actions =
        [
            new("العيادات", "قائمة العيادات النشطة", true),
            new("المراجعات", "قريباً", false),
            new("الكوادر", "قريباً", false),
            new("التقارير", "قريباً", false)
        ]
    };

    private static readonly FacilityTypeProfile Warehouse = new()
    {
        Eyebrow = "WAREHOUSE",
        OverviewLead = "واجهة المستودع: مخزون، نواقص، وطلبات معلّقة.",
        Nav =
        [
            new("overview", "الرئيسية", true, true),
            new("stock", "المخزون", true, true),
            new("low", "نواقص", false, true),
            new("requests", "الطلبات", false, true),
            new("shipments", "الشحنات", false, false)
        ],
        Kpis =
        [
            new("أصناف", "5,610"),
            new("منخفضة", "9"),
            new("طلبات معلّقة", "19"),
            new("شحنات اليوم", "4")
        ],
        Actions =
        [
            new("المخزون", "تصفح الأصناف", true),
            new("نواقص", "قريباً", false),
            new("الطلبات", "قريباً", false),
            new("الشحنات", "قريباً", false)
        ]
    };

    private static readonly FacilityTypeProfile Hospital = new()
    {
        Eyebrow = "HOSPITAL",
        OverviewLead = "واجهة مشفى عامة. مشفى حرستا له واجهة تفصيلية مستقلة.",
        Nav =
        [
            new("overview", "الرئيسية", true, true),
            new("wards", "الأقسام", true, true),
            new("er", "الإسعاف", false, true),
            new("staff", "الكوادر", false, true),
            new("reports", "تقارير", false, false)
        ],
        Kpis =
        [
            new("أسرّة", "—"),
            new("عيادات", "—"),
            new("عمليات اليوم", "—"),
            new("كوادر", "—")
        ],
        Actions =
        [
            new("الأقسام", "هيكل أولي", true),
            new("الإسعاف", "قريباً", false),
            new("الكوادر", "قريباً", false),
            new("التقارير", "قريباً", false)
        ]
    };

    public static IReadOnlyList<(string Name, string Detail)> PrisonServices { get; } =
    [
        ("إسعاف النقطة", "النقطة الطبية"),
        ("عيادة سنية أولية", "النقطة الطبية"),
        ("عيادة داخلية أولية", "النقطة الطبية"),
        ("صيدلية أولية", "النقطة الطبية"),
        ("إسعاف المشفى", "المشفى"),
        ("عيادة داخلية متقدمة", "المشفى"),
        ("غرفة العمليات", "المشفى"),
        ("أسرّة العناية", "6 أسرّة"),
        ("أسرّة الاستشفاء", "24 سريرًا"),
        ("صيدلية المشفى", "المشفى")
    ];

    public static IReadOnlyList<(string Name, string Detail)> CenterClinics { get; } =
    [
        ("عيادة عامة", "نشطة"),
        ("أطفال", "نشطة"),
        ("نسائية", "نشطة"),
        ("أسنان", "نشطة"),
        ("داخلية", "نشطة"),
        ("إسعاف أولي", "نشطة")
    ];

    public static IReadOnlyList<(string Name, string Detail)> WarehouseStock { get; } =
    [
        ("مستهلكات إسعافية", "رصيد جيد"),
        ("أدوية مزمنة", "رصيد متوسط"),
        ("مستلزمات عمليات", "منخفض"),
        ("محاليل وريدية", "رصيد جيد"),
        ("مواد تعقيم", "منخفض")
    ];
}
