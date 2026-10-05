using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Roulette;

/// <summary>
/// Case-opening roulette. Per player; <see cref="SpinAll"/> spins one for everyone, each with its own draw.
/// </summary>
public interface IRouletteService
{
    /// <summary>
    /// Draws the winner now and plays the spin. A spin while one is running ends the first (its callback
    /// gets <see cref="RouletteResult.Interrupted"/>) and starts the new one once the screen has left.
    /// Returns a dead handle when no item can be drawn.
    /// </summary>
    IUiHandle Spin(CCSPlayerController player, RouletteOptions options);

    void SpinAll(RouletteOptions options);

    /// <summary>Ends the spin, running or not. Null layout = every roulette layout.</summary>
    void Hide(CCSPlayerController player, RouletteLayout? layout = null);

    void HideAll(RouletteLayout? layout = null);
}
