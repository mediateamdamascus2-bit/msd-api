namespace MedicalServices.Web.Services;

public static class UnitOpsCatalog
{
    public static IReadOnlyList<string> EventKinds { get; } =
    [
        "إصابة", "مرض", "حادث", "نقل عاجل", "أخرى"
    ];

    public static IReadOnlyList<string> EventSeverities { get; } =
    [
        "عادي", "متوسط", "عاجل"
    ];

    public static IReadOnlyList<string> NeedCategories { get; } =
    [
        "أدوية", "مستلزمات", "كوادر", "صيانة", "أخرى"
    ];

    public static IReadOnlyList<string> NeedPriorities { get; } =
    [
        "عادي", "عاجل"
    ];
}
