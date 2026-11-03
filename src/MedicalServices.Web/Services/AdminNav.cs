namespace MedicalServices.Web.Services;

public sealed record AdminNavItem(string Key, string LabelAr, string Path, bool Ready);

public static class AdminNav
{
    public static IReadOnlyList<AdminNavItem> Items { get; } =
    [
        new("home", "الرئيسية", "/admin", Ready: true),
        new("facilities", "المواقع والواجهات", "/admin/facilities", Ready: true),
        new("events", "الأحداث الطبية", "/admin/section/events", Ready: false),
        new("fleet", "الأسطول", "/admin/section/fleet", Ready: false),
        new("map", "الخريطة", "/admin/section/map", Ready: false),
        new("reports", "التقارير", "/admin/section/reports", Ready: false),
        new("settings", "الإعدادات", "/admin/section/settings", Ready: false)
    ];
}
