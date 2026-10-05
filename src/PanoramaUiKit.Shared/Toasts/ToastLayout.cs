using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Toasts;

/// <summary>
/// A toast layout: a fixed pool of <see cref="Depth"/> cards per placement. Contract "toast"
/// (docs/CONTRACTS.md). Declare it once, reuse the instance:
///
/// <code>
/// public static readonly ToastLayout Neon = new()
/// {
///     Name = "neon_toast",   // panorama/layout/custom_game/neon_toast.xml, root id neon_toast_root
///     Placements = [ToastPlacement.TopRight, ToastPlacement.BottomCenter],
///     Depth = 3,
/// };
/// </code>
/// </summary>
public sealed class ToastLayout : UiLayout
{
    public static readonly IReadOnlyList<ToastPlacement> AllPlacements = Enum.GetValues<ToastPlacement>();

    /// <summary>The <c>dur-*</c> classes the bundled stylesheet defines.</summary>
    public static readonly IReadOnlyList<int> DefaultDurations = [2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30];

    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public ToastLayout()
    {
        ExitSeconds = 0.3f;
    }

    public override string Component => "toast";

    /// <summary>Placements the layout has a stack for. A toast asking for another uses the first one.</summary>
    public IReadOnlyList<ToastPlacement> Placements { get; init; } = AllPlacements;

    /// <summary>Cards per placement. When all are taken the oldest is pushed out.</summary>
    public int Depth { get; init; } = 4;

    /// <summary><c>dur-N</c> classes the bar knows. A toast's duration snaps to the nearest.</summary>
    public IReadOnlyList<int> Durations { get; init; } = DefaultDurations;
}