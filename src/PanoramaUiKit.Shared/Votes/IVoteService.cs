using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Votes;

/// <summary>
/// The screen of a vote, per player. The kit draws what it is given and reports clicks; it keeps no
/// tally. The loop is yours: <see cref="Show"/>, a click in <see cref="VoteOptions.OnVote"/>, record it,
/// <see cref="Update"/> for everyone with the new counts, <see cref="HideAll"/> when it is over.
/// </summary>
public interface IVoteService
{
    /// <summary>Opens the vote for the player (restarting its countdown), or redraws it if it is already open.</summary>
    IUiHandle Show(CCSPlayerController player, VoteOptions options);

    void ShowAll(VoteOptions options);

    /// <summary>Redraws an open vote with new counts. Does nothing for a player who has it closed.</summary>
    void Update(CCSPlayerController player, VoteOptions options);

    /// <summary><see cref="Update"/> for every player who has it open.</summary>
    void UpdateAll(VoteOptions options);

    /// <summary>Null layout = every vote layout.</summary>
    void Hide(CCSPlayerController player, VoteLayout? layout = null);

    void HideAll(VoteLayout? layout = null);
}
