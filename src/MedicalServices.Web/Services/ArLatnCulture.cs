using System.Globalization;

namespace MedicalServices.Web.Services;

/// <summary>Arabic culture with Latin digits (0–9) for dates and numbers.</summary>
public static class ArLatnCulture
{
    public static CultureInfo Instance { get; } = Create();

    private static CultureInfo Create()
    {
        try
        {
            return CultureInfo.GetCultureInfo("ar-SY-u-nu-latn");
        }
        catch (CultureNotFoundException)
        {
            var c = CultureInfo.CreateSpecificCulture("ar-SY");
            c.NumberFormat.DigitSubstitution = DigitShapes.None;
            c.NumberFormat.NativeDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
            return c;
        }
    }

    /// <summary>Replace Eastern/Persian digits if a formatter still emits them.</summary>
    public static string ToLatnDigits(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        Span<char> buf = stackalloc char[s.Length];
        for (var i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            if (ch is >= '\u0660' and <= '\u0669') buf[i] = (char)('0' + (ch - '\u0660'));
            else if (ch is >= '\u06F0' and <= '\u06F9') buf[i] = (char)('0' + (ch - '\u06F0'));
            else buf[i] = ch;
        }
        return new string(buf);
    }

    public static string Format(DateTime value, string format) =>
        ToLatnDigits(value.ToString(format, Instance));

    public static string Format(DateOnly value, string format) =>
        ToLatnDigits(value.ToString(format, Instance));
}
