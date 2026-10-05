using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Panels;

/// <summary>
/// A modal panel with pages in a left menu. Clicking a page switches the content; rows carry their
/// own callbacks. Takes the mouse while open.
///
/// Opened with a build function, the panel is rebuilt on open, after every click and once a second,
/// so live content (a player list) stays current without pushing anything. Every write is cached
/// per viewer: a steady panel costs nothing per refresh.
/// </summary>
public interface IPanelService
{
    /// <summary>A live panel, rebuilt from <paramref name="build"/>. Replaces whatever panel the player had on that layout.</summary>
    IUiHandle Open(CCSPlayerController player, Func<CCSPlayerController, PanelOptions> build, int page = 0);

    /// <summary>A fixed panel. Page switches still work; nothing is rebuilt.</summary>
    IUiHandle Open(CCSPlayerController player, PanelOptions options, int page = 0);

    /// <summary>Rebuilds and repaints now, rather than waiting for the next second.</summary>
    void Refresh(CCSPlayerController player, PanelLayout? layout = null);

    /// <summary>Selects a page of the open panel.</summary>
    void ShowPage(CCSPlayerController player, int page, PanelLayout? layout = null);

    /// <summary>Null layout = every panel layout.</summary>
    void Close(CCSPlayerController player, PanelLayout? layout = null);
}
