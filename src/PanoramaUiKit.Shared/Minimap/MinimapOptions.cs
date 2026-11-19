using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// The minimap of one player: which map, where, how big, and whether the kit draws the player's own arrow.
/// Zones and global marks are set on the layout (<see cref="IMinimapService.SetZone"/>,
/// <see cref="IMinimapService.SetGlobalMark"/>) and show on every player's map.
/// <see cref="UiContent.Style"/> and <see cref="UiContent.Classes"/> go on <c>mm_frame</c>;
/// <see cref="UiContent.Texts"/> are written as <c>mm_&lt;key&gt;</c>.
/// </summary>
public sealed record MinimapOptions : UiContent
{
    /// <summary>
    /// <c>map-&lt;name&gt;</c> on <c>mm_map</c>: see <see cref="MinimapMaps"/>. Defaults to the server's current map;
    /// an unknown name draws a plain background.
    /// </summary>
    public string? Map { get; init; }

    /// <summary>World rectangle of the picture. Null = <see cref="MinimapMaps.Official"/> for <see cref="Map"/>; without either nothing is placed.</summary>
    public MinimapBounds? Bounds { get; init; }

    public MinimapPlacement Placement { get; init; } = MinimapPlacement.TopRight;

    public MinimapSize Size { get; init; } = MinimapSize.Medium;

    /// <summary>
    /// 1 = the whole map. Above 1, the map is zoomed by that much (snapped to <see cref="MinimapLayout.Zooms"/>: 1.5, 2,
    /// 3, 4) and centred on the player's arrow, following it; markers keep their size. Without an arrow (slot 0)
    /// it centres the map.
    /// </summary>
    public float Zoom { get; init; } = 1f;

    /// <summary>Slot <c>{s:mm_title}</c> under the map: a map or area name. Blank collapses it.</summary>
    public string? Title { get; init; }

    /// <summary>The kit puts the player in slot 0 (<see cref="MinimapNames.Self"/>, turned by the view) and keeps it there.</summary>
    public bool FollowSelf { get; init; } = true;

    /// <summary>
    /// Size of the player's own arrow and its halo, 1 = the stylesheet's. Snapped to 10 % steps from 0.5 to 2.5
    /// (class <c>self-NNN</c> on <c>mm_frame</c>, NNN in percent). Other markers keep their size.
    /// </summary>
    public float SelfScale { get; init; } = 1f;

    /// <summary>Style of the player's own arrow, see <see cref="MinimapArrows"/>. Null or <c>classic</c> = the gold arrow in its ring.</summary>
    public string? SelfArrow { get; init; }

    /// <summary>Seconds between two reads of the player's position while following.</summary>
    public float FollowInterval { get; init; } = 0.05f;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required MinimapLayout Layout { get; init; }
}
