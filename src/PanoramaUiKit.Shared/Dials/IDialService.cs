using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Dials;

/// <summary>
/// Radial dials: cooldowns, charges, channels. One per placement and player. Showing again on a
/// visible pushed dial only moves the ring and rewrites the text, so it is safe to call every tick.
/// </summary>
public interface IDialService
{
    IUiHandle Show(CCSPlayerController player, DialOptions options);

    /// <summary>Same dial to every connected human.</summary>
    void ShowAll(DialOptions options);

    /// <summary>Hides this player's dial at that placement, or every dial when null. Null layout = on every dial layout.</summary>
    void Hide(CCSPlayerController player, DialPlacement? placement = null, DialLayout? layout = null);

    void HideAll(DialPlacement? placement = null, DialLayout? layout = null);
}
