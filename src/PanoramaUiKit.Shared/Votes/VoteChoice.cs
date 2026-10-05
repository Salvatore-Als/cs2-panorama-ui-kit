using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Votes;

/// <summary>
/// One choice of a vote. <see cref="Label"/> in <c>{s:vt_choice_&lt;n&gt;_label}</c>, <see cref="Sub"/> in
/// <c>{s:vt_choice_&lt;n&gt;_sub}</c>, <see cref="Votes"/> as the count and, against the total, as the
/// percentage and the fill of the bar. <see cref="Style"/> is <c>style-*</c> on <c>vt_choice_&lt;n&gt;</c>.
/// The kit only draws the numbers it is given: counting the votes is the caller's.
/// </summary>
/// <param name="Label">Main line of the choice.</param>
/// <param name="Votes">Votes for it so far. Zero or less draws as zero.</param>
/// <param name="Sub">Second line. Blank collapses it.</param>
/// <param name="Style">See <see cref="UiStyle"/>.</param>
public sealed record VoteChoice(string Label, int Votes = 0, string? Sub = null, string Style = UiStyle.Neutral);
