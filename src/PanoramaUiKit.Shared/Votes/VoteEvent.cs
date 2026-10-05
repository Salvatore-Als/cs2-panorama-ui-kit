using CounterStrikeSharp.API.Core;

namespace PanoramaUiKit.Shared.Votes;

/// <summary>A click on a choice. The kit does not count it: record it, then <see cref="IVoteService.Update"/>.</summary>
/// <param name="Player">Who clicked.</param>
/// <param name="Index">Position of the choice in <see cref="VoteOptions.Choices"/>, whatever page it was on.</param>
public sealed record VoteEvent(CCSPlayerController Player, int Index);
