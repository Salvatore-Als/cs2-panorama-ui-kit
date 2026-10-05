using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.KillFeed;

/// <summary>Contract "killfeed": a pool of <see cref="Rows"/> rows, ids <c>kf_*</c>.</summary>
public sealed class KillFeedLayout : UiLayout
{
    public override string Component => "killfeed";

    /// <summary>Rows <c>kf_0</c>..<c>kf_{Rows-1}</c>, row 0 on top.</summary>
    public int Rows { get; init; } = 5;
}
