using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Targets;

/// <summary>A target layout: one label per <see cref="Positions"/> entry around the crosshair. Contract "target".</summary>
public sealed class TargetLayout : UiLayout
{
    public static readonly IReadOnlyList<TargetPosition> AllPositions = Enum.GetValues<TargetPosition>();

    public TargetLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "target";

    /// <summary>Positions the layout has a label for. A text asking for another uses the first one.</summary>
    public IReadOnlyList<TargetPosition> Positions { get; init; } = AllPositions;
}
