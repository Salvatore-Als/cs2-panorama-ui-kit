namespace PanoramaUiKit.Internal;

/// <summary>Normalisation of <c>UiContent.Texts</c>, shared by the per-viewer and the shared text writers.</summary>
internal static class Extras
{
    /// <summary>Sanitised, de-duplicated keys with their trimmed values, in the caller's order.</summary>
    public static IEnumerable<(string Key, string Value)> Normalize(IReadOnlyDictionary<string, string>? texts)
    {
        if (texts == null)
        {
            yield break;
        }

        HashSet<string> seen = [];
        foreach ((string rawKey, string value) in texts)
        {
            string? key = Names.Token(rawKey);
            if (key != null && seen.Add(key))
            {
                yield return (key, value?.Trim() ?? string.Empty);
            }
        }
    }
}
