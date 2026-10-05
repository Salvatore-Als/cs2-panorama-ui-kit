using System.Text;

namespace PanoramaUiKit.Internal;

/// <summary>Turns caller strings into class names and slot suffixes Panorama will accept.</summary>
internal static class Names
{
    /// <summary>Lowercase [a-z0-9-_]; anything else dropped. Null when nothing is left.</summary>
    public static string? Token(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        StringBuilder builder = new StringBuilder(value.Length);
        foreach (char c in value.Trim().ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_')
            {
                builder.Append(c);
            }
        }

        if (builder.Length == 0)
        {
            return null;
        }

        return builder.ToString();
    }

    public static string? StyleClass(string? style)
    {
        string? token = Token(style);
        if (token == null)
        {
            return null;
        }

        return $"style-{token}";
    }

    /// <summary><c>style-&lt;name&gt;</c>, falling back to <c>style-neutral</c> for a blank or invalid name.</summary>
    /// <summary><c>string.Format</c> with one number; a broken pattern shows the bare number.</summary>
    public static string Format(string pattern, long value)
    {
        try
        {
            return string.Format(pattern, value);
        }
        catch (FormatException)
        {
            return value.ToString();
        }
    }

    public static string StyleOrNeutral(string? style) => StyleClass(style) ?? "style-neutral";

    /// <summary>Distinct sanitised classes, in order.</summary>
    public static List<string> Classes(IReadOnlyList<string>? classes)
    {
        List<string> result = new();
        if (classes == null)
        {
            return result;
        }

        foreach (string value in classes)
        {
            string? token = Token(value);
            if (token != null && !result.Contains(token))
            {
                result.Add(token);
            }
        }

        return result;
    }
}
