using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Recap;

/// <summary>
/// End of match screen: placement, headline, the player's name and stat tiles. Per player, since every
/// player has their own stats. A Show while the screen is up repaints it in place: a tile whose value
/// changed pulses.
/// </summary>
public interface IRecapService
{
    IUiHandle Show(CCSPlayerController player, RecapOptions options);

    /// <summary>The same screen for every human, each with their own name when <see cref="RecapOptions.Player"/> is null.</summary>
    void ShowAll(RecapOptions options);

    /// <summary>
    /// Rewrites the value of tile <paramref name="index"/> (0-based) on the screen up for this player, and pulses it.
    /// Nothing happens when no recap is up or the tile is not shown. Null layout = every recap layout.
    /// </summary>
    void UpdateStat(CCSPlayerController player, int index, string value, RecapLayout? layout = null);

    /// <summary>Null layout = every recap layout.</summary>
    void Hide(CCSPlayerController player, RecapLayout? layout = null);

    void HideAll(RecapLayout? layout = null);
}
