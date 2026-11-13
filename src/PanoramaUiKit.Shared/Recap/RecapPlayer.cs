using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Recap;

/// <summary>
/// One player row of the recap (<see cref="RecapOptions.Players"/>): a name, an optional line beside it and a few
/// stats on the same line. For a duo or a squad that won together. Only the first
/// <see cref="RecapLayout.RowStats"/> stats are shown.
/// </summary>
public sealed record RecapPlayer(string Name, IReadOnlyList<RecapStat> Stats, string? Sub = null, string Style = UiStyle.Neutral);
