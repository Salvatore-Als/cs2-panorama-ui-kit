using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// A minimap in a corner: the map picture, the zone circles (current in red, next in white) and markers.
///
/// Two layers. <b>Global</b>, set on the layout, the same on every map that shows it: the zones
/// (<see cref="SetZone"/>, with a shrink the client animates by itself) and global markers (points of interest).
/// <b>Per player</b>: the map itself (<see cref="Show"/>), the player's own arrow (followed by the kit) and
/// markers only they see (a teammate). A player mark wins over a global one in the same slot.
///
/// Global calls take a layout, or null for every minimap layout: one <c>SetZone(null, ...)</c> draws the zone on the
/// corner map (<c>uikit_minimap</c>) and on the big one (<c>uikit_bigmap</c>) alike. Null reaches the layouts the kit
/// knows by then: preload them first.
/// Positions snap to the layout's grid; nothing is sent while a marker stays in its cell.
/// </summary>
public interface IMinimapService
{
    /// <summary>
    /// Tells the kit how to draw a map that is not an official one: by its name (<c>dz_arcadias</c>), the picture and its
    /// bounds. From then on, a <see cref="Show"/> on that map (the server's, when <see cref="MinimapOptions.Map"/> is
    /// null, or named) uses them, on every minimap layout. Typically a slot your addon fills:
    /// <code>kit.Minimap.RegisterMap("dz_arcadias", new MinimapMap(MinimapMaps.Slot(1), MinimapBounds.FromOverview(-4096, 4096, 8)));</code>
    /// Register from OnAllPluginsLoaded; registering a name again replaces it.
    /// </summary>
    void RegisterMap(string map, MinimapMap picture);

    /// <summary>How a map is drawn: registered, else official (<see cref="MinimapMaps.Official"/>), else null (nothing is placed on it).</summary>
    MinimapMap? FindMap(string map);

    /// <summary>Shows or updates this player's map. It stays until hidden, through a round restart.</summary>
    IUiHandle Show(CCSPlayerController player, MinimapOptions options);

    void ShowAll(MinimapOptions options);

    /// <summary>
    /// Sets a zone circle on every map of the layout; null takes it off. With <paramref name="shrinkSeconds"/>,
    /// the circle moves and shrinks from where it is to <paramref name="zone"/> over that time, run by the client.
    /// </summary>
    void SetZone(MinimapLayout? layout, MinimapZoneId id, MinimapZone? zone, float shrinkSeconds = 0f);

    /// <summary>A marker on every map of the layout (a point of interest).</summary>
    void SetGlobalMark(MinimapLayout? layout, MinimapMark mark);

    void ClearGlobalMark(MinimapLayout? layout, int slot);

    /// <summary>A marker only this player sees (a teammate). Wins over a global one in the same slot.</summary>
    void SetMark(CCSPlayerController player, MinimapLayout layout, MinimapMark mark);

    void ClearMark(CCSPlayerController player, MinimapLayout layout, int slot);

    /// <summary>
    /// Follows <paramref name="target"/> with a marker on every map of the layout: position and view read by the kit,
    /// dead / disconnected / reconnected handled (see <see cref="MinimapTrack"/>). <see cref="ClearGlobalMark"/> on the
    /// slot stops it.
    /// </summary>
    void TrackPlayer(MinimapLayout? layout, CCSPlayerController target, MinimapTrack track);

    /// <summary>Same, on <paramref name="viewer"/>'s map only (a teammate). <see cref="ClearMark"/> on the slot stops it.</summary>
    void TrackPlayerFor(CCSPlayerController viewer, MinimapLayout layout, CCSPlayerController target, MinimapTrack track);

    /// <summary>
    /// Follows every player of <paramref name="targets"/> on every map of the layout (null = every minimap layout), one
    /// marker each in the slots of <paramref name="list"/>, in list order, a colour each. Call again with the new list
    /// whenever it changes: players no longer in it are taken off. An empty list clears the range.
    /// </summary>
    void TrackPlayers(MinimapLayout? layout, IReadOnlyList<CCSPlayerController> targets, MinimapTrackList? list = null);

    /// <summary>Same, on <paramref name="viewer"/>'s map only (their squad, the players they spectate).</summary>
    void TrackPlayersFor(CCSPlayerController viewer, MinimapLayout layout, IReadOnlyList<CCSPlayerController> targets,
                         MinimapTrackList? list = null);

    /// <summary>True while a map is on this player's screen, on that layout or on any when null.</summary>
    bool IsShowing(CCSPlayerController player, MinimapLayout? layout = null);

    /// <summary>Takes it off this player's screen. Null layout = every minimap layout.</summary>
    void Hide(CCSPlayerController player, MinimapLayout? layout = null);

    void HideAll(MinimapLayout? layout = null);

    /// <summary>Hides every map, and drops the zones and global marks of the layout (a new match). Null = every layout.</summary>
    void Reset(MinimapLayout? layout = null);
}
