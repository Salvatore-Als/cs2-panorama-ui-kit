using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Progress;

/// <summary>
/// Progress bars. One per placement and player. Showing again on a visible pushed bar only moves the
/// fill and rewrites the text, so it is safe to call every tick: unchanged values cost nothing.
/// </summary>
public interface IProgressService
{
    IUiHandle Show(CCSPlayerController player, ProgressOptions options);

    /// <summary>Same bar to every connected human.</summary>
    void ShowAll(ProgressOptions options);

    /// <summary>Hides this player's bar at that placement, or every bar when null. Null layout = on every progress layout.</summary>
    void Hide(CCSPlayerController player, ProgressPlacement? placement = null, ProgressLayout? layout = null);

    void HideAll(ProgressPlacement? placement = null, ProgressLayout? layout = null);
}
