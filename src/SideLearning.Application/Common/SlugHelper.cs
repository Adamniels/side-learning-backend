using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SideLearning.Application.Common;

public static partial class SlugHelper
{
    public static string Slugify(string name)
    {
        var lower = name.Trim().ToLowerInvariant();
        var sb = new StringBuilder(lower.Length);
        foreach (var c in lower.Normalize(NormalizationForm.FormD))
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var normalized = sb.ToString().Normalize(NormalizationForm.FormC);
        normalized = SlugInvalidChars().Replace(normalized, "-");
        normalized = Regex.Replace(normalized, @"\s+", "-");
        normalized = Regex.Replace(normalized, "-{2,}", "-").Trim('-');
        return string.IsNullOrEmpty(normalized) ? "topic" : normalized;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled)]
    private static partial Regex SlugInvalidChars();
}
