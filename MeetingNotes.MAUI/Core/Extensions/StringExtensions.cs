using System.Globalization;

namespace MeetingNotes.MAUI.Core.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + suffix;
    }

    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }
}
