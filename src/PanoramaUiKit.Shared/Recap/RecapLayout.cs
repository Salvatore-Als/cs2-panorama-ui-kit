using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Recap;

/// <summary>Contract "recap": one screen, ids <c>rc_*</c>, a pool of <see cref="Stats"/> stat tiles.</summary>
public sealed class RecapLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public RecapLayout()
    {
        ExitSeconds = 0.5f;
    }

    public override string Component => "recap";

    /// <summary>Tiles <c>rc_stat_0</c>..<c>rc_stat_{Stats-1}</c>.</summary>
    public int Stats { get; init; } = 8;

    /// <summary>Player rows <c>rc_row_0</c>..<c>rc_row_{Rows-1}</c>, for <see cref="RecapOptions.Players"/>.</summary>
    public int Rows { get; init; } = 4;

    /// <summary>Stats per player row, <c>rc_row_&lt;r&gt;_stat_0</c>..<c>rc_row_&lt;r&gt;_stat_{RowStats-1}</c>.</summary>
    public int RowStats { get; init; } = 4;
}
