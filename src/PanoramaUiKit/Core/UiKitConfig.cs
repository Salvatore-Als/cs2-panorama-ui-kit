using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace PanoramaUiKit.Core;

/// <summary>
/// addons/counterstrikesharp/configs/plugins/PanoramaUiKit/PanoramaUiKit.json
///
/// There is no default layout: every call names the layout file it draws on. A layout is spawned the
/// first time something uses it.
/// </summary>
public sealed class UiKitConfig : BasePluginConfig
{
    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 2;

    /// <summary>Permission for css_uikit_status and css_uikit_clear.</summary>
    public string AdminPermission { get; set; } = "@css/root";
}
