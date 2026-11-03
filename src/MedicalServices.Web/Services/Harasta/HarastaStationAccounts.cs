namespace MedicalServices.Web.Services.Harasta;

/// <summary>Independent station logins for Harasta clinics (and shared helpers).</summary>
public static class HarastaStationAccounts
{
    public sealed record ClinicAccount(string UserName, string DisplayNameAr, string Clinic);

    public static IReadOnlyList<ClinicAccount> Clinics { get; } =
    [
        new("clinic-internal", "محطة عيادة الداخلية", "الداخلية"),
        new("clinic-cardio", "محطة عيادة القلبية", "القلبية"),
        new("clinic-peds", "محطة عيادة الأطفال", "الأطفال"),
        new("clinic-neonatal", "محطة عيادة حديثي الولادة", "حديثي الولادة"),
        new("clinic-obgyn", "محطة عيادة النسائية والتوليد", "النسائية والتوليد"),
        new("clinic-derm", "محطة عيادة الجلدية", "الجلدية"),
        new("clinic-gensurg", "محطة عيادة الجراحة العامة", "الجراحة العامة"),
        new("clinic-ortho", "محطة عيادة الجراحة العظمية", "الجراحة العظمية"),
        new("clinic-eye", "محطة عيادة العينية", "العينية"),
        new("clinic-ent", "محطة عيادة الأذنية", "الأذنية"),
        new("clinic-uro", "محطة عيادة البولية", "البولية"),
        new("clinic-neuro", "محطة عيادة العصبية", "العصبية"),
        new("clinic-dental", "محطة عيادة السنية", "السنية"),
        new("clinic-kidney", "محطة عيادة الكلية وغسيل الكلى", "الكلية وغسيل الكلى"),
        new("clinic-physio", "محطة عيادة العلاج الفيزيائي", "العلاج الفيزيائي"),
        new("clinic-er", "محطة الإسعاف", "الإسعاف"),
    ];

    public static string ClinicHomePath(string clinic) =>
        $"/stations/harasta/clinic/{clinic}";

    public static string? ClinicFromHomePath(string? homePath)
    {
        if (string.IsNullOrWhiteSpace(homePath))
            return null;

        const string prefix = "/stations/harasta/clinic/";
        var path = Uri.UnescapeDataString(homePath.Trim()).TrimEnd('/');
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var clinic = path[prefix.Length..].Trim();
        return HarastaData.Clinics.Contains(clinic) ? clinic : null;
    }
}
