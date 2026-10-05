using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Common;

/// <summary>
/// A layout file and the facts about it the server cannot discover: the client renders the XML, the
/// server only ever addresses ids, <c>{s:...}</c> variables and classes by name.
///
/// <para>A layout is named by its file name only: <c>uikit_toast</c> is
/// <c>panorama/layout/custom_game/uikit_toast.xml</c> in the addon, compiled to
/// <c>panorama/layout/custom_game/uikit_toast.vxml_c</c>, which is what the kit loads. That folder is the
/// only one CS2 searches for custom HUD layouts.</para>
///
/// <para>The root panel (one level under the anonymous root) must have id <c>&lt;name&gt;_root</c>.
/// Dialog variables are addressed through the root panel id, so two layouts sharing a root id share
/// every text slot - one toast's title would appear on the other. Deriving it from the file name keeps
/// it unique. Sharing slot names (<c>t_tr_0_title</c>) between layouts is fine and is the point: that
/// is what makes a custom toast a drop-in.</para>
/// </summary>
public abstract class UiLayout
{
    public const string LayoutFolder = "panorama/layout/custom_game";

    private readonly string _name = string.Empty;

    /// <summary>
    /// File name without folder or extension: <c>uikit_toast</c>. A full path or an extension is
    /// tolerated and stripped.
    /// </summary>
    public required string Name
    {
        get => _name;
        init => _name = Normalize(value);
    }

    /// <summary>Id of the panel one level under the anonymous root: always <c>&lt;name&gt;_root</c>.</summary>
    [JsonIgnore]
    public string RootPanelId => $"{_name}_root";

    /// <summary>Compiled resource path the client loads.</summary>
    [JsonIgnore]
    public string Path => $"{LayoutFolder}/{_name}.vxml_c";

    /// <summary>Length of the layout's exit transition plus a margin. The panel is released after it.</summary>
    public float ExitSeconds { get; init; } = 0.3f;

    /// <summary>Delay between lifting <c>hidden</c> and adding <c>show</c>, so the entry transition has a start state.</summary>
    public float EntrySeconds { get; init; } = 0.06f;

    /// <summary>Contract name, as in contracts/*.json.</summary>
    [JsonIgnore]
    public abstract string Component { get; }

    public override string ToString() => $"{Component}:{_name}";

    private static string Normalize(string value)
    {
        string name = value.Trim().Replace('\\', '/');
        int slash = name.LastIndexOf('/');
        if (slash >= 0)
        {
            name = name[(slash + 1)..];
        }

        foreach (string extension in new[] { ".vxml_c", ".vxml", ".xml" })
        {
            if (name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                name = name[..^extension.Length];
                break;
            }
        }

        return name;
    }
}
