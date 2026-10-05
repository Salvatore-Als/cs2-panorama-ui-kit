using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Victory;

/// <summary>
/// End of round screen. <see cref="UiContent.Style"/> (the outcome colour) and
/// <see cref="UiContent.Classes"/> go on <c>v_screen</c>; <see cref="UiContent.Texts"/> are written as
/// <c>v_&lt;key&gt;</c>.
/// </summary>
public sealed record VictoryOptions : UiContent
{
    /// <summary>Slot <c>{s:v_title}</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Slot <c>{s:v_subtitle}</c>. Blank collapses it.</summary>
    public string? Subtitle { get; init; }

    /// <summary>Slot <c>{s:v_kicker}</c>. Blank collapses it.</summary>
    public string? Kicker { get; init; }

    /// <summary>Only the first layout <c>Chips</c> are shown. Empty adds <c>hidden</c> on <c>v_chips</c>.</summary>
    public IReadOnlyList<VictoryChip> Chips { get; init; } = [];

    /// <summary>
    /// Countdown under the card, ticked by the kit: <c>{s:v_restart_eta}</c> = <see cref="RestartFormat"/>
    /// of the seconds left. 0 adds <c>hidden</c> on <c>v_restart</c>. Typically mp_round_restart_delay.
    /// </summary>
    public int RestartSeconds { get; init; }

    /// <summary>Slot <c>{s:v_restart_label}</c>.</summary>
    public string RestartLabel { get; init; } = "Next round in";

    /// <summary><c>string.Format</c> pattern, <c>{0}</c> = seconds left.</summary>
    public string RestartFormat { get; init; } = "{0}s";

    /// <summary>Seconds on screen. Zero or less stays until hidden (or the round restart closes it).</summary>
    public float Seconds { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required VictoryLayout Layout { get; init; }
}
