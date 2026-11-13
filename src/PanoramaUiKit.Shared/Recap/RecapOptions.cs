using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Recap;

/// <summary>
/// End of match screen for one player. <see cref="UiContent.Style"/> (the outcome colour) and
/// <see cref="UiContent.Classes"/> go on <c>rc_screen</c>; <see cref="UiContent.Texts"/> are written as
/// <c>rc_&lt;key&gt;</c>.
/// </summary>
public sealed record RecapOptions : UiContent
{
    /// <summary>Slot <c>{s:rc_title}</c>: the headline.</summary>
    public required string Title { get; init; }

    /// <summary>Slot <c>{s:rc_kicker}</c>, above the placement. Blank collapses it.</summary>
    public string? Kicker { get; init; }

    /// <summary>Final placement, slot <c>{s:rc_place}</c> as <see cref="PlaceFormat"/>. Null adds <c>hidden</c> on <c>rc_place_row</c>.</summary>
    public int? Place { get; init; }

    /// <summary><c>string.Format</c> pattern, <c>{0}</c> = <see cref="Place"/>.</summary>
    public string PlaceFormat { get; init; } = "#{0}";

    /// <summary>Players in the match, slot <c>{s:rc_place_total}</c> as <see cref="TotalFormat"/>. Null collapses it.</summary>
    public int? Total { get; init; }

    /// <summary><c>string.Format</c> pattern, <c>{0}</c> = <see cref="Total"/>.</summary>
    public string TotalFormat { get; init; } = "/ {0}";

    /// <summary>Slot <c>{s:rc_player}</c>. Null = the viewer's own name.</summary>
    public string? Player { get; init; }

    /// <summary>Slot <c>{s:rc_player_sub}</c>, under the name (a team, a squad). Blank collapses it.</summary>
    public string? PlayerSub { get; init; }

    /// <summary>Only the first layout <c>Stats</c> are shown. Empty adds <c>hidden</c> on <c>rc_stats</c>. See <see cref="RecapStats"/>.</summary>
    public IReadOnlyList<RecapStat> Stats { get; init; } = [];

    /// <summary>
    /// A duo or a squad, one row each (name and stats on one line), in place of <see cref="Player"/> and
    /// <see cref="Stats"/>: those are hidden while this has rows. Only the first layout <c>Rows</c> are shown.
    /// </summary>
    public IReadOnlyList<RecapPlayer> Players { get; init; } = [];

    /// <summary>
    /// Countdown under the strip, ticked by the kit: <c>{s:rc_restart_eta}</c> = <see cref="RestartFormat"/>
    /// of the seconds left. 0 adds <c>hidden</c> on <c>rc_restart</c>.
    /// </summary>
    public int RestartSeconds { get; init; }

    /// <summary>Slot <c>{s:rc_restart_label}</c>.</summary>
    public string RestartLabel { get; init; } = "Next match in";

    /// <summary><c>string.Format</c> pattern, <c>{0}</c> = seconds left.</summary>
    public string RestartFormat { get; init; } = "{0}s";

    /// <summary>Seconds on screen. Zero or less stays until hidden (or the round restart closes it).</summary>
    public float Seconds { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required RecapLayout Layout { get; init; }
}
