using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Progress;

/// <summary>
/// A progress bar, driven one of two ways:
/// <list type="bullet">
/// <item><b>Pushed</b> (<see cref="Seconds"/> is zero): you call Show with a new <see cref="Value"/> whenever it
/// changes. The bar eases to it. It stays until hidden.</item>
/// <item><b>Timed</b> (<see cref="Seconds"/> above zero): one call, and the client runs the bar by itself for that
/// long. No further writes. It hides itself when done.</item>
/// </list>
/// </summary>
public sealed record ProgressOptions : UiContent
{
    /// <summary>Text above the bar, left. Slot <c>{s:pg_&lt;bar&gt;_label}</c>. Collapsed when blank.</summary>
    public string? Label { get; init; }

    /// <summary>Text above the bar, right. Slot <c>{s:pg_&lt;bar&gt;_value}</c>. Collapsed when blank. Wins over <see cref="ShowPercent"/>.</summary>
    public string? ValueText { get; init; }

    /// <summary>Writes <see cref="Value"/> as a percentage in the value slot. Pushed bars only.</summary>
    public bool ShowPercent { get; init; }

    /// <summary>Pushed bars: 0 to 1, clamped.</summary>
    public float Value { get; init; }

    /// <summary>Timed bars: how long it runs. Snaps to the layout's nearest <c>dur-N</c>. Zero or less = pushed.</summary>
    public float Seconds { get; init; }

    /// <summary>Timed bars: fills up or drains.</summary>
    public ProgressDirection Direction { get; init; } = ProgressDirection.Fill;

    /// <summary>Falls back to the layout's first placement when the layout does not have this one.</summary>
    public ProgressPlacement Placement { get; init; } = ProgressPlacement.Crosshair;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required ProgressLayout Layout { get; init; }
}
