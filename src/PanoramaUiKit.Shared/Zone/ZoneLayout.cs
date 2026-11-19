using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Zone;

/// <summary>
/// A zone layout: one status widget (<c>zn_status</c>) and one alert (<c>zn_alert</c>). The bar of the status
/// drains from a <see cref="Steps"/> start ladder over a <c>dur-N</c> class. Contract "zone" (docs/CONTRACTS.md).
/// </summary>
public sealed class ZoneLayout : UiLayout
{
    /// <summary>The <c>dur-*</c> classes the bundled stylesheet defines: every 5 s to 2 min, then every 30 s to 5 min.</summary>
    public static readonly IReadOnlyList<int> DefaultDurations =
    [
        5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120,
        150, 180, 210, 240, 270, 300,
    ];

    /// <summary>Exit default matches the bundled alert's transition, plus a margin.</summary>
    public ZoneLayout()
    {
        ExitSeconds = 0.4f;
    }

    public override string Component => "zone";

    /// <summary><c>step-0</c>..<c>step-{Steps}</c>: where the bar starts, as a share of the phase still to run.</summary>
    public int Steps { get; init; } = 20;

    /// <summary><c>dur-N</c> classes the bar knows. The time left snaps to the nearest; the countdown text stays exact.</summary>
    public IReadOnlyList<int> Durations { get; init; } = DefaultDurations;
}
