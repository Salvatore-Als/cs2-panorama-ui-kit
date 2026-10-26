using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Screamers;

/// <summary>
/// A full-screen picture with its sound, gone after <see cref="ScreamerOptions.Seconds"/>. The picture is
/// one class of the stylesheet; the sound (<see cref="ScreamerOptions.Sound"/>) is played on the player's
/// client by the server, in the same tick. A new screamer on a player who already has one swaps the
/// picture in place (and plays the new sound); the same one again only extends it, without its sound.
/// Check <see cref="IsShowing"/> first to keep one screamer per player.
/// </summary>
public interface IScreamerService
{
    IUiHandle Show(CCSPlayerController player, ScreamerOptions options);

    /// <summary>True while a screamer is on this player's screen, on that layout or on any when null.</summary>
    bool IsShowing(CCSPlayerController player, ScreamerLayout? layout = null);

    /// <summary>Takes it off this player's screen. Null layout = on every screamer layout.</summary>
    void Hide(CCSPlayerController player, ScreamerLayout? layout = null);

    void HideAll(ScreamerLayout? layout = null);
}
