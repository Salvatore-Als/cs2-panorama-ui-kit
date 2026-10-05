using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Roulette;

/// <summary>
/// One possible outcome of a roulette. Shown as a card on the strip: <see cref="Name"/> in
/// <c>{s:r_cell_&lt;n&gt;_name}</c>, <see cref="Sub"/> in <c>{s:r_cell_&lt;n&gt;_sub}</c>,
/// <see cref="Style"/> as <c>style-*</c> on <c>r_cell_&lt;n&gt;</c> (the rarity colour).
/// </summary>
/// <param name="Name">Main line of the card.</param>
/// <param name="Sub">Second line. When no item of the roulette has one, the layout collapses the line.</param>
/// <param name="Style">See <see cref="UiStyle"/>.</param>
/// <param name="Weight">
/// Relative chance. 1 against 3 is one in four. Zero or less takes the item out of the draw. The same
/// weights set how often each item appears on the strip while it spins.
/// </param>
/// <param name="Tag">Yours: comes back untouched in <see cref="RouletteResult.Item"/>.</param>
public sealed record RouletteItem(string Name, string? Sub = null, string Style = UiStyle.Neutral,
                                  float Weight = 1f, object? Tag = null);
