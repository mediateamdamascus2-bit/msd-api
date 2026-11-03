namespace MedicalServices.Web.Services.Hasia;

/// <summary>Independent station logins for Hasia prison clinic.</summary>
public static class HasiaStationAccounts
{
    public sealed record StationAccount(string UserName, string LabelAr, string HomePath);

    public static IReadOnlyList<StationAccount> Stations { get; } =
    [
        new("hasia-intake", "محطة استقبال النزيل", "/stations/hasia/intake"),
        new("hasia-triage", "محطة الفرز", "/stations/hasia/triage"),
        new("hasia-cases", "محطة الحالات", "/stations/hasia/cases"),
        new("hasia-ph-int", "محطة الصيدلية الداخلية", "/stations/hasia/ph-internal"),
        new("hasia-ph-ext", "محطة الصيدلية الخارجية", "/stations/hasia/ph-external"),
    ];
}
