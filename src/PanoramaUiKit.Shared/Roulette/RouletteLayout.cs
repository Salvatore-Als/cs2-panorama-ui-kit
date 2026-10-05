using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Roulette;

/// <summary>
/// Contract "roulette": one strip of <see cref="Cells"/> cards, ids <c>r_*</c>. The stylesheet scrolls
/// the strip far enough to centre card <see cref="WinnerCell"/> under the marker, so those two numbers
/// must be the ones the layout was built with (the <c>cells=</c> and <c>winner=</c> in its marker).
/// </summary>
public sealed class RouletteLayout : UiLayout
{
    public static readonly IReadOnlyList<int> DefaultDurations = [1, 2, 3, 4, 5, 6, 8, 10];

    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public RouletteLayout()
    {
        ExitSeconds = 0.5f;
    }

    public override string Component => "roulette";

    /// <summary>Cards <c>r_cell_0</c>..<c>r_cell_{Cells-1}</c> on the strip.</summary>
    public int Cells { get; init; } = 31;

    /// <summary>The card the strip stops on. Always the winner; the other cards are filler.</summary>
    public int WinnerCell { get; init; } = 24;

    /// <summary><c>dur-N</c> classes the strip knows. <see cref="RouletteOptions.Seconds"/> snaps to the nearest.</summary>
    public IReadOnlyList<int> Durations { get; init; } = DefaultDurations;

    /// <summary>
    /// Share of the duration the strip runs at full speed (<c>spin</c>) before <c>settle</c> slows it onto the
    /// winner's card. The stylesheet's <c>dur-N</c> transitions are split the same way.
    /// </summary>
    public float SettleAfterFraction { get; init; } = 0.6f;

    /// <summary>Wait after <c>show</c> before the strip starts: the card's entry transition plus a beat.</summary>
    public float SpinDelaySeconds { get; init; } = 0.5f;
}
