using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Roulette;

/// <summary>
/// A case-opening roulette: a strip of cards scrolls past a marker and slows down until it stops on
/// the winner. The winner is drawn on the server when the spin starts, from <see cref="RouletteItem.Weight"/>;
/// the strip is only the show. <see cref="UiContent.Style"/> and <see cref="UiContent.Classes"/> go on
/// <c>r_screen</c>; <see cref="UiContent.Texts"/> are written as <c>r_&lt;key&gt;</c>.
/// </summary>
public sealed record RouletteOptions : UiContent
{
    /// <summary>Slot <c>{s:r_title}</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Slot <c>{s:r_kicker}</c>. Blank collapses it.</summary>
    public string? Kicker { get; init; }

    /// <summary>The possible outcomes. At least one with a weight above zero.</summary>
    public required IReadOnlyList<RouletteItem> Items { get; init; }

    /// <summary>
    /// How long the strip scrolls, which is its speed: the distance is fixed, so 2 seconds is three times
    /// as fast as 6. Snaps to the nearest of <see cref="RouletteLayout.Durations"/> (1, 2, 3, 4, 5, 6, 8, 10
    /// by default), the durations the stylesheet has a transition for.
    /// </summary>
    public float Seconds { get; init; } = 4f;

    /// <summary>
    /// How long the result stays on screen once the strip has stopped. Zero or less keeps it until
    /// the handle is cleared (or the round restarts).
    /// </summary>
    public float HoldSeconds { get; init; } = 3f;

    /// <summary>Slot <c>{s:r_result_label}</c>, above the result line.</summary>
    public string ResultLabel { get; init; } = "You got";

    /// <summary>
    /// Called once, when the strip has stopped (or with <see cref="RouletteResult.Interrupted"/> when it
    /// never will). Runs on the server thread, from the kit's timer.
    /// </summary>
    public Action<RouletteResult>? OnResult { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required RouletteLayout Layout { get; init; }
}
