using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Victory;

/// <summary>Contract "victory": one screen, ids <c>v_*</c>, a pool of <see cref="Chips"/> chips.</summary>
public sealed class VictoryLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public VictoryLayout()
    {
        ExitSeconds = 0.5f;
    }

    public override string Component => "victory";

    /// <summary>Chips <c>v_chip_0</c>..<c>v_chip_{Chips-1}</c>.</summary>
    public int Chips { get; init; } = 4;
}
