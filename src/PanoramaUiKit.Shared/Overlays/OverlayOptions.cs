using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Overlays;

/// <summary>
/// A full-screen effect (<see cref="OverlayEffects"/>): frozen, burning, poisoned, your own logo... One at a time per
/// player on a layout; showing another replaces it.
/// </summary>
public sealed record OverlayOptions : UiContent
{
    /// <summary>
    /// Which effect: <c>fx-&lt;name&gt;</c> on <c>ov_fx</c>. See <see cref="OverlayEffects"/>, or
    /// <see cref="OverlayEffects.Custom"/> for a picture your addon ships. Blank hides the overlay.
    /// </summary>
    public required string Effect { get; init; }

    /// <summary>Seconds on screen. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; }

    /// <summary>Whether it stays on screen when the player dies. Most states end with the player, so false by default.</summary>
    public bool KeepOnDeath { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required OverlayLayout Layout { get; init; }
}
