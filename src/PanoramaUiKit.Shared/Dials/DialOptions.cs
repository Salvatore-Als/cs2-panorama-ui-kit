using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Progress;

namespace PanoramaUiKit.Shared.Dials;

/// <summary>
/// A radial dial, driven one of two ways:
/// <list type="bullet">
/// <item><b>Pushed</b> (<see cref="Seconds"/> is zero): call Show with a new <see cref="Value"/> whenever it
/// changes. The ring eases to it. It stays until hidden.</item>
/// <item><b>Timed</b> (<see cref="Seconds"/> above zero): one call, and the client sweeps the ring by itself for
/// that long. It hides itself when done.</item>
/// </list>
/// </summary>
public sealed record DialOptions : UiContent
{
    /// <summary>Text under the ring. Slot <c>{s:dl_&lt;dial&gt;_label}</c>. Collapsed when blank.</summary>
    public string? Label { get; init; }

    /// <summary>Text in the middle of the ring. Slot <c>{s:dl_&lt;dial&gt;_value}</c>. Collapsed when blank. Wins over <see cref="ShowPercent"/>.</summary>
    public string? ValueText { get; init; }

    /// <summary>Writes <see cref="Value"/> as a percentage in the middle. Pushed dials only.</summary>
    public bool ShowPercent { get; init; }

    /// <summary>Pushed dials: 0 to 1, clamped.</summary>
    public float Value { get; init; }

    /// <summary>Timed dials: how long it runs. Snaps to the layout's nearest <c>dur-N</c>. Zero or less = pushed.</summary>
    public float Seconds { get; init; }

    /// <summary>Timed dials: fills up (a charge) or drains (a cooldown).</summary>
    public ProgressDirection Direction { get; init; } = ProgressDirection.Drain;

    public DialSize Size { get; init; } = DialSize.Medium;

    /// <summary>Falls back to the layout's first placement when the layout does not have this one.</summary>
    public DialPlacement Placement { get; init; } = DialPlacement.Crosshair;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required DialLayout Layout { get; init; }
}
