using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Progress;

/// <summary>
/// A progress layout: one bar per <see cref="Placements"/> entry. The fill is a ladder of
/// <see cref="Steps"/> clip classes (a pushed value) plus <c>dur-N</c> classes (a bar that runs by
/// itself). Contract "progress" (docs/CONTRACTS.md).
/// </summary>
public sealed class ProgressLayout : UiLayout
{
    public static readonly IReadOnlyList<ProgressPlacement> AllPlacements = Enum.GetValues<ProgressPlacement>();

    /// <summary>The <c>dur-*</c> classes the bundled stylesheet defines.</summary>
    public static readonly IReadOnlyList<int> DefaultDurations = [1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30, 45, 60];

    /// <summary>A bar has no transition on the panel itself, so there is no exit to wait for.</summary>
    public ProgressLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "progress";

    /// <summary>Bars the layout has. A bar asking for another placement uses the first one.</summary>
    public IReadOnlyList<ProgressPlacement> Placements { get; init; } = AllPlacements;

    /// <summary><c>step-0</c>..<c>step-{Steps}</c> classes: the resolution of a pushed value is 1/Steps.</summary>
    public int Steps { get; init; } = 50;

    /// <summary><c>dur-N</c> classes the fill knows. A timed bar's duration snaps to the nearest.</summary>
    public IReadOnlyList<int> Durations { get; init; } = DefaultDurations;
}
