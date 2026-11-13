using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Comms;

/// <summary>
/// A radio transmission on the left of the screen: a face as a glitching hologram, a name and a line of
/// text. A Show while one is up swaps face and text in place, behind a short burst of static, and
/// restarts the timer.
/// </summary>
public interface ICommsService
{
    IUiHandle Show(CCSPlayerController player, CommsOptions options);

    void ShowAll(CommsOptions options);

    /// <summary>
    /// Rewrites the text of the transmission on screen, in place: no burst, no timer restart. For a line typed out
    /// word by word. False when none is up. Null layout = every comms layout.
    /// </summary>
    bool SetText(CCSPlayerController player, string text, CommsLayout? layout = null);

    /// <summary>Plays the power-off, then closes. Null layout = every comms layout.</summary>
    void Hide(CCSPlayerController player, CommsLayout? layout = null);

    void HideAll(CommsLayout? layout = null);
}
