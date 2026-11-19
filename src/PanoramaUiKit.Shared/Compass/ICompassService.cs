using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Compass;

/// <summary>
/// A compass strip at the top of the screen, per player. It follows the player's view by itself
/// (<see cref="CompassOptions.Follow"/>); pins mark world points or fixed headings on it. A Show on a player who
/// already has it replaces the options in place. It stays until hidden, through a round restart.
/// </summary>
public interface ICompassService
{
    IUiHandle Show(CCSPlayerController player, CompassOptions options);

    /// <summary>Replaces the pins of a compass already up, keeping everything else. False when none is up.</summary>
    bool SetPins(CCSPlayerController player, IReadOnlyList<CompassPin> pins, CompassLayout? layout = null);

    /// <summary>True while a compass is on this player's screen, on that layout or on any when null.</summary>
    bool IsShowing(CCSPlayerController player, CompassLayout? layout = null);

    /// <summary>Takes it off this player's screen. Null layout = on every compass layout.</summary>
    void Hide(CCSPlayerController player, CompassLayout? layout = null);

    void HideAll(CompassLayout? layout = null);
}
