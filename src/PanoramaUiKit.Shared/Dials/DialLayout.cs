using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Dials;

/// <summary>
/// A dial layout: one ring per <see cref="Placements"/> entry. The fill is a ladder of
/// <see cref="Steps"/> radial clip classes (a pushed value) plus <c>dur-N</c> classes (a ring that runs
/// by itself). Contract "dial" (docs/CONTRACTS.md).
/// </summary>
public sealed class DialLayout : UiLayout
{
    public static readonly IReadOnlyList<DialPlacement> AllPlacements = Enum.GetValues<DialPlacement>();

    /// <summary>The <c>dur-*</c> classes the bundled stylesheet defines.</summary>
    public static readonly IReadOnlyList<int> DefaultDurations = [1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30, 45, 60];

    /// <summary>A dial has no transition on the panel itself, so there is no exit to wait for.</summary>
    public DialLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "dial";

    /// <summary>Dials the layout has. A dial asking for another placement uses the first one.</summary>
    public IReadOnlyList<DialPlacement> Placements { get; init; } = AllPlacements;

    /// <summary><c>step-0</c>..<c>step-{Steps}</c> classes: a pushed value moves in steps of 360 / Steps degrees.</summary>
    public int Steps { get; init; } = 36;

    /// <summary><c>dur-N</c> classes the fill knows. A timed dial's duration snaps to the nearest.</summary>
    public IReadOnlyList<int> Durations { get; init; } = DefaultDurations;
}
