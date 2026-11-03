namespace MedicalServices.Web.Services;

public static class PatientNameRules
{
    public static string RequireTripleName(string? raw, string? tag = null)
    {
        var name = string.Join(
            ' ',
            (raw ?? string.Empty)
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            throw new InvalidOperationException(
                "أدخل الاسم الثلاثي كاملاً (ثلاثة أجزاء على الأقل)" + (tag ?? ".") );

        return name;
    }
}
