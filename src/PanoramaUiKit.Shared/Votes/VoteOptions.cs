using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Votes;

/// <summary>
/// The screen of a vote: a question, its choices with their live counts, a countdown. Only the screen:
/// who voted for what, when the vote ends and who wins is the caller's. A click arrives in
/// <see cref="OnVote"/>; the caller records it and shows the new state with <see cref="IVoteService.Update"/>.
/// <see cref="UiContent.Style"/> (accent) and <see cref="UiContent.Classes"/> go on <c>vt_screen</c>;
/// <see cref="UiContent.Texts"/> are written as <c>vt_&lt;key&gt;</c>.
/// </summary>
public sealed record VoteOptions : UiContent
{
    /// <summary>Slot <c>{s:vt_title}</c>: the question.</summary>
    public required string Title { get; init; }

    /// <summary>Slot <c>{s:vt_kicker}</c>. Blank collapses it.</summary>
    public string? Kicker { get; init; }

    /// <summary>
    /// Every choice of the vote. The layout shows <see cref="VoteLayout.Choices"/> of them at a time: with
    /// more, a pager under the list (<c>{s:vt_page}</c> = "1 / 2") moves between pages. Percentages and the
    /// total always count all of them.
    /// </summary>
    public required IReadOnlyList<VoteChoice> Choices { get; init; }

    /// <summary>Index of the choice this viewer picked: marked <c>selected</c>. Null for none.</summary>
    public int? Selected { get; init; }

    /// <summary>
    /// Countdown, ticked by the kit from the moment of <see cref="IVoteService.Show"/>: <c>{s:vt_time}</c> =
    /// <see cref="TimeFormat"/> of the seconds left. At zero the screen is locked: clicks stop reaching
    /// <see cref="OnVote"/>. Zero or less: no countdown. <see cref="IVoteService.Update"/> does not restart it.
    /// </summary>
    public float Seconds { get; init; }

    /// <summary>
    /// False hides the close button (<c>no-close</c> on <c>vt_screen</c>): the player cannot dismiss the
    /// screen, so close it yourself when the vote is over or cast (<see cref="IVoteService.Hide"/>), the
    /// mouse is theirs until then.
    /// </summary>
    public bool Closable { get; init; } = true;

    /// <summary>Shows the choices but takes no click: <c>locked</c> on <c>vt_screen</c> (vote over, or already cast).</summary>
    public bool Locked { get; init; }

    /// <summary><c>string.Format</c> pattern, <c>{0}</c> = seconds left.</summary>
    public string TimeFormat { get; init; } = "{0}s";

    /// <summary>Slot <c>{s:vt_total}</c>. <c>string.Format</c> pattern, <c>{0}</c> = votes in all.</summary>
    public string TotalFormat { get; init; } = "{0} votes";

    /// <summary>A click on a choice, unless the screen is locked. Runs on the server thread.</summary>
    public Action<VoteEvent>? OnVote { get; init; }

    /// <summary>
    /// Called once when the screen leaves this player's screen (close button, hide, round restart,
    /// disconnect). The vote itself goes on: it is not the kit's.
    /// </summary>
    public Action<CCSPlayerController>? OnClose { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required VoteLayout Layout { get; init; }
}
