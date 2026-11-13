using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// A minimap layout: the map <c>mm_map</c>, two zone circles (<c>mm_zone_cur</c>, <c>mm_zone_next</c>) and a pool of
/// <see cref="Marks"/> markers. Positions are a class ladder on a <see cref="Grid"/> x <see cref="Grid"/> grid,
/// radii a ladder of <see cref="Radii"/> steps. Contract "minimap" (docs/CONTRACTS.md).
/// </summary>
public sealed class MinimapLayout : UiLayout
{
    /// <summary>The <c>dur-*</c> classes the bundled stylesheet defines: every 5 s to 2 min, every 30 s to 5 min, then every minute to 10.</summary>
    public static readonly IReadOnlyList<int> DefaultDurations =
    [
        5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120,
        150, 180, 210, 240, 270, 300, 360, 420, 480, 540, 600,
    ];

    /// <summary>Shown and closed at once: a map has no entry to play.</summary>
    public MinimapLayout()
    {
        EntrySeconds = 0f;
        ExitSeconds = 0f;
    }

    public override string Component => "minimap";

    /// <summary><c>x-0</c>..<c>x-{Grid-1}</c> and the same for <c>y-*</c>. The bundled stylesheet has 64 (4px cells on its 256px map).</summary>
    public int Grid { get; init; } = 64;

    /// <summary>Markers <c>mm_mark_0</c>..<c>mm_mark_{Marks-1}</c>. Marks beyond it are not drawn.</summary>
    public int Marks { get; init; } = 16;

    /// <summary><c>r-0</c>..<c>r-{Radii}</c>: a zone's radius, <c>r-{Radii}</c> being the width of the whole map.</summary>
    public int Radii { get; init; } = 64;

    /// <summary><c>zoom-N</c> classes the bundled stylesheet defines, in percent. <see cref="MinimapOptions.Zoom"/> snaps to the nearest.</summary>
    public IReadOnlyList<int> Zooms { get; init; } = [150, 200, 300, 400];

    /// <summary><c>dur-N</c> classes a zone shrink can run over. The time left snaps to the nearest.</summary>
    public IReadOnlyList<int> Durations { get; init; } = DefaultDurations;
}
