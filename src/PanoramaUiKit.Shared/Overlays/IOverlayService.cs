using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Overlays;

/// <summary>
/// A full-screen effect over the player's view: frozen, burning, knocked down, poisoned, or one of your own (a logo, a
/// splash) through <see cref="OverlayEffects.Custom"/>. One per player and layout; showing another replaces it.
/// </summary>
public interface IOverlayService
{
    IUiHandle Show(CCSPlayerController player, OverlayOptions options);

    /// <summary>Same effect to every connected human.</summary>
    void ShowAll(OverlayOptions options);

    /// <summary>True while an effect is up for this player, on that layout or on any when null.</summary>
    bool IsShowing(CCSPlayerController player, OverlayLayout? layout = null);

    /// <summary>Takes this player's effect off. Null layout = on every overlay layout.</summary>
    void Hide(CCSPlayerController player, OverlayLayout? layout = null);

    void HideAll(OverlayLayout? layout = null);
}
