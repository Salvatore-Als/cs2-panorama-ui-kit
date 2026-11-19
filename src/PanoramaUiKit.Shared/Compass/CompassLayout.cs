using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Compass;

/// <summary>
/// A compass layout: one strip, <c>cp_strip</c>, moved by an <c>hd-N</c> class ladder, and a pool of
/// <see cref="Pins"/> pins. Contract "compass" (docs/CONTRACTS.md).
/// </summary>
public sealed class CompassLayout : UiLayout
{
    /// <summary>The bar has no entry or exit transition: it is shown and closed at once.</summary>
    public CompassLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "compass";

    /// <summary>Pins <c>cp_pin_0</c>..<c>cp_pin_{Pins-1}</c>. Pins beyond it are not drawn.</summary>
    public int Pins { get; init; } = 4;
}
