using System.Numerics;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Compass;

/// <summary>
/// A mark on the compass, at the heading of a world point (<see cref="Position"/>, game units) seen from the
/// player, or at a fixed heading (<see cref="Bearing"/>, for a direction with no point: "the wind", "north-east").
/// <see cref="Kind"/> is <c>kind-*</c> on <c>cp_pin_&lt;n&gt;</c> (<see cref="CompassNames"/>), <see cref="Style"/> its colour.
/// </summary>
/// <param name="Position">World point the pin points at, in game units.</param>
/// <param name="Kind">Shape, <c>kind-*</c> (<see cref="CompassNames"/>).</param>
/// <param name="Style">Colour, <c>style-*</c> (<see cref="UiStyle"/>).</param>
/// <param name="ShowDistance">Writes the distance to <see cref="Position"/> under the pin. Ignored with a <see cref="Bearing"/>.</param>
public sealed record CompassPin(
    Vector3 Position,
    string Kind = CompassNames.Dot,
    string Style = UiStyle.Neutral,
    bool ShowDistance = true)
{
    /// <summary>Fixed heading, 0 = north, clockwise. Wins over <see cref="Position"/> when set.</summary>
    public float? Bearing { get; init; }

    /// <summary>A pin at a fixed heading, without a distance.</summary>
    public static CompassPin AtBearing(float bearing, string kind = CompassNames.Dot, string style = UiStyle.Neutral) =>
        new(Vector3.Zero, kind, style, ShowDistance: false) { Bearing = bearing };
}
