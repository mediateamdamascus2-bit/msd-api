namespace MedicalServices.Web.Services.Harasta;

public static class HarastaData
{
    public const string FacilityId = "harasta";
    public const string TitleAr = "مشفى الشرطة في حرستا";

    public static IReadOnlyList<HarastaNavItem> Nav { get; } =
    [
        new("overview", "الرئيسية", true, true),
        new("structure", "الهيكلية", true, true),
        new("floors", "الطوابق", true, true),
        new("staff", "الكوادر", true, true),
        new("reception", "الاستقبال", true, false),
        new("clinics", "العيادات", true, false),
        new("depts", "أقسام التحويل", true, false),
        new("files", "ملفات المرضى", true, false),
        new("fleet", "الأسطول", true, false),
        new("stores", "المستودعات", true, false),
        new("assets", "الأصول", true, false),
        new("vision", "الرؤية 2030", true, false)
    ];

    public static IReadOnlyList<string> StoreSections { get; } =
    [
        "الصيدلية المركزية", "المستلزمات الطبية", "المخبر", "الأشعة",
        "غسيل الكلى", "التغذية والمطبخ", "المفروشات"
    ];

    public static IReadOnlyList<string> AssetDepartments { get; } =
    [
        "الأشعة", "الإسعاف", "غسيل الكلى", "القلبية", "الأطفال",
        "العناية", "العمليات", "العلاج الفيزيائي", "الخدمات"
    ];

    public static IReadOnlyList<KpiItem> Kpis { get; } =
    [
        new("اختصاصًا طبيًا", "35"),
        new("سريرًا مُحصى", "246"),
        new("سرير عناية وحواضن", "45"),
        new("غرفة عمليات ومخاض", "16"),
        new("كادرًا مذكورًا", "663"),
        new("جهاز غسيل كلية", "11")
    ];

    public static IReadOnlyList<StatItem> OpsStats { get; } =
    [
        new("عمليات تخصصية", "30–40", "يوميًا"),
        new("مراجعو العيادات", "~500", "يوميًا"),
        new("ولادات", "10", "يوميًا"),
        new("تحاليل مخبرية", "40,000", "شهريًا"),
        new("غسيل كلى", "120", "جلسة/شهر"),
        new("علاج فيزيائي", "150", "حالة/أسبوع")
    ];

    public static IReadOnlyList<DomainGroup> Domains { get; } =
    [
        new("or", "العمليات والمخاض",
        [
            new("غرف العمليات التخصصية", "6 غرف · 30–40 عملية يوميًا", "متوافر"),
            new("غرف العمليات الإسعافية", "3 غرف", "متوافر"),
            new("غرف العمليات النسائية", "غرفتان", "متوافر"),
            new("غرف المخاض", "غرفتان", "متوافر"),
            new("غرفة عمليات زرع الكلية", "غرفة واحدة", "يحتاج تثبيت")
        ]),
        new("icu", "العناية والحواضن",
        [
            new("العناية القلبية CCU", "6 أسرّة", "متوافر"),
            new("العناية الجراحية SICU", "5 أسرّة", "متوافر"),
            new("العناية الداخلية MICU", "6 أسرّة", "متوافر"),
            new("العناية المتوسطة HDU", "8 أسرّة", "متوافر"),
            new("عناية حديثي الولادة NICU", "5 حاضنات", "قيد التجهيز")
        ]),
        new("ward", "أقسام الإقامة",
        [
            new("الجراحة ذكور", "44 سريرًا · الطابق الأول", "متوافر"),
            new("النسائية والتوليد", "20 سريرًا · الطابق الثاني", "متوافر"),
            new("إقامة الأطفال", "20 سريرًا", "متوافر"),
            new("الداخلية إناث", "20 سريرًا", "متوافر"),
            new("الداخلية ذكور", "20 سريرًا", "يحتاج تثبيت")
        ]),
        new("er", "الإسعاف",
        [
            new("أسرّة الإسعاف", "15 سريرًا", "متوافر"),
            new("غرفة الإنعاش", "لحالات الإصابات", "متوافر"),
            new("سيارات الإخلاء", "سيارتان", "قيد التجهيز")
        ]),
        new("clinic", "العيادات والأسنان",
        [
            new("العيادات التخصصية", "35 عيادة · يعمل منها 22", "متوافر"),
            new("العيادات السنية", "3 عيادات", "متوافر"),
            new("العلاج الفيزيائي", "~150 حالة أسبوعيًا", "متوافر")
        ])
    ];

    public static IReadOnlyList<FloorRow> Floors { get; } =
    [
        new("الأول", "", "جناح الجراحة ذكور", "44 سريرًا"),
        new("الثاني", "", "النسائية والتوليد", "20 سريرًا"),
        new("الثالث", "الغربي", "الأطفال", "20 سريرًا"),
        new("الرابع", "الغربي", "الداخلية إناث", "20 سريرًا"),
        new("الخامس", "", "الجناح الخاص VIP و VVIP", "إقامة خاصة"),
        new("الخامس", "الشرقي", "الجناح الاحترازي", "22 سريرًا"),
        new("السادس", "", "غسيل الكلية ووحدة الزرع", "11 جهاز غسيل"),
        new("السابع", "", "المطعم", "مرفق خدمي")
    ];

    public static IReadOnlyList<StaffRow> Staff { get; } =
    [
        new("أطباء اختصاصيون", 106),
        new("استشاريون", 11),
        new("أطباء مقيمون", 140),
        new("التمريض", 326),
        new("الخدمة والتعقيم", 80)
    ];

    public static IReadOnlyList<string> Clinics { get; } =
    [
        "الداخلية", "القلبية", "الأطفال", "حديثي الولادة", "النسائية والتوليد", "الجلدية",
        "الجراحة العامة", "الجراحة العظمية", "العينية", "الأذنية", "البولية", "العصبية", "السنية",
        "الكلية وغسيل الكلى", "العلاج الفيزيائي", "الإسعاف"
    ];

    public static IReadOnlyList<string> AdminUnits { get; } =
    [
        "إدارة الخدمات الطبية", "قيادة شرطة دمشق", "قيادة شرطة ريف دمشق",
        "إدارة السجون", "إدارة المرور", "إدارة الهجرة والجوازات", "الأمن الجنائي", "أخرى"
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

    public static IReadOnlyList<string> Departments { get; } =
    [
        "أسرّة الاستشفاء", "العناية", "العمليات", "المخبر", "الصيدلية", "الأشعة"
    ];

    /// <summary>Capacity for live department board (beds / rooms). Null = queue-only dest.</summary>
    public static IReadOnlyDictionary<string, int?> DepartmentCapacity { get; } =
        new Dictionary<string, int?>
        {
            ["أسرّة الاستشفاء"] = 124,
            ["العناية"] = 45,
            ["العمليات"] = 16,
            ["المخبر"] = null,
            ["الصيدلية"] = null,
            ["الأشعة"] = null
        };

    public static IReadOnlyDictionary<string, string> DepartmentTone { get; } =
        new Dictionary<string, string>
        {
            ["أسرّة الاستشفاء"] = "cyan",
            ["العناية"] = "red",
            ["العمليات"] = "amber",
            ["المخبر"] = "green",
            ["الصيدلية"] = "blue",
            ["الأشعة"] = "teal"
        };

    public static IReadOnlyList<BedCapacityRow> BedCapacity { get; } =
    [
        new("أقسام الإقامة", 124, "cyan"),
        new("العناية والحواضن", 45, "blue"),
        new("الجناح الاحترازي", 22, "amber"),
        new("الإسعاف", 15, "red")
    ];

    public const string BedCapacityNote = "دراسة لرفع الطاقة الاستيعابية الكلية من 250 إلى 400 سرير";

    public static IReadOnlyList<VisionYear> VisionRoadmap { get; } =
    [
        new("2026", "التأسيس الرقمي والتخطيط للتوسع", "cyan",
        [
            "رفع العنايات إلى 30 سريراً (15 عامة + 15 قلبية)",
            "تطوير برنامج زرع الكلى",
            "البدء بكتلة الأطفال والنسائية",
            "إقرار الهيكل التنظيمي والسياسات المكتوبة",
            "اعتماد أولي من البورد السوري لـ70٪ من الاختصاصات",
            "البدء الفعلي بنظام EMAR",
            "ابتعاث أول دفعة (5–10 أطباء)",
            "أول مؤتمر طبي على مستوى الجمهورية"
        ]),
        new("2027", "التوسع والاعتماد", "blue",
        [
            "افتتاح كتلة الأطفال والنسائية والتوليد",
            "التخطيط لكتلتي القلب والكلى",
            "تفعيل «المأجور الخيري» كاملاً",
            "اعتماد أولي من البورد العربي",
            "شهادة اعتماد محلية لجودة الرعاية",
            "المرحلة الأولى من EMAR وربط مستشفيات الوزارة",
            "ابتعاث 10 أطباء و10 تمريض"
        ]),
        new("2028", "التخصص والريادة", "teal",
        [
            "افتتاح كتلة القلب والأوعية والقثطرة",
            "افتتاح كتلة الكلى وزراعة الأعضاء",
            "البدء بمركز التأهيل والصحة النفسية",
            "نشر أبحاث في مجلات عالمية محكمة",
            "أول مؤتمر طبي دولي",
            "اكتمال نظام الإحالة الإلكتروني"
        ]),
        new("2029", "مركز طبي تعليمي رائد", "green",
        [
            "افتتاح مختبر المحاكاة Simulation Lab",
            "استكمال الكتلة الرابعة: الأطراف الصناعية والتأهيل والصحة النفسية"
        ]),
        new("2030", "التميز", "amber",
        [
            "مستشفى تعليمي خدمي بامتياز",
            "اعتماد دولي JCI أو ما يعادله",
            "تحول رقمي كامل ومستشفيات افتراضية"
        ])
    ];

    public static IReadOnlyList<VisionBlock> VisionBlocks { get; } =
    [
        new("أ", "مركز الأطفال والنسائية والتوليد", "cyan",
        [
            "عيادات تخصصية وإسعاف نسائي وأطفال",
            "عمليات نسائية وغرف مخاض وولادة",
            "MICU للأمهات · PICU · عناية متوسطة",
            "عناية حديثي الولادة وحواضن متطورة",
            "قسم طفل الأنبوب والعقم"
        ]),
        new("ب", "مركز القلب والأوعية الدموية", "red",
        [
            "عمليات القلب المفتوح",
            "مختبر القسطرة التداخلية",
            "CCU ومختبر كهربية القلب EP",
            "اختبار الجهد والعيادات القلبية"
        ]),
        new("ج", "مركز الكلى وزراعة الأعضاء", "teal",
        [
            "غسيل دموي وبريتوني متطور",
            "عمليات زراعة الكلى وعنايتها",
            "مختبرات المناعة والأنسجة",
            "الديال المنزلي والمسالك وتأهيل المثانة"
        ]),
        new("د", "مركز التأهيل الشامل والصحة النفسية", "amber",
        [
            "ورشة الأطراف الصناعية",
            "العلاج الفيزيائي المتخصص والتأهيل العصبي",
            "الصحة النفسية للكوادر",
            "برامج خاصة لمرضى السجون"
        ])
    ];

    public static VisionSwot VisionSwot { get; } = new(
        [
            "كادر طبي متنوع في 35 اختصاصاً",
            "إنجازات سريعة خلال 7 أشهر",
            "برامج جودة وضبط عدوى رائدة",
            "مساحة 52 دونماً تسمح بالتوسع"
        ],
        [
            "بيروقراطية الصرف والشراء",
            "عدم وضوح الميزانية وتقسيمها",
            "ضعف المستوى العلمي لبعض الكوادر",
            "غياب النظام الطبي الإلكتروني EMR"
        ],
        [
            "ميزانية بأبواب منفصلة (تشغيل، بنى، تقنية، تدريب)",
            "صلاحيات أوسع في التعاقد والشراء",
            "تمويل عاجل لمشروع EMAR",
            "ابتعاث وتأهيل لغوي ورواتب تنافسية"
        ]);
}

public sealed record HarastaNavItem(string Key, string LabelAr, bool Ready, bool MobilePrimary);
public sealed record KpiItem(string Label, string Value);
public sealed record StatItem(string Label, string Value, string Unit);
public sealed record DomainGroup(string Key, string Title, IReadOnlyList<UnitRow> Units);
public sealed record UnitRow(string Name, string Capacity, string Status);
public sealed record FloorRow(string Floor, string Side, string Unit, string Note);
public sealed record StaffRow(string Label, int Value);
public sealed record VisionYear(string Year, string Title, string Tone, IReadOnlyList<string> Items);
public sealed record VisionBlock(string Key, string Title, string Tone, IReadOnlyList<string> Items);
public sealed record VisionSwot(IReadOnlyList<string> Strengths, IReadOnlyList<string> Challenges, IReadOnlyList<string> Needs);
public sealed record BedCapacityRow(string Label, int Beds, string Tone);
