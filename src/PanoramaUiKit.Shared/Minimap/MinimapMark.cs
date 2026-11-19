namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// A marker on the minimap, in slot <see cref="Slot"/> (<c>mm_mark_&lt;slot&gt;</c>), at a world point.
/// A player's own mark in a slot wins over a global one in the same slot.
/// </summary>
/// <param name="Slot">0 to <see cref="MinimapLayout.Marks"/> - 1. Slot 0 is the player's own arrow while
/// <see cref="MinimapOptions.FollowSelf"/> is on.</param>
/// <param name="Kind"><c>kind-&lt;name&gt;</c>, see <see cref="MinimapNames"/>.</param>
/// <param name="X">World X, game units.</param>
/// <param name="Y">World Y, game units.</param>
/// <param name="Yaw">Game yaw in degrees for an arrow (0 = +X, 90 = +Y); null = no turn.</param>
/// <param name="State"><c>state-&lt;name&gt;</c>, see <see cref="MinimapNames"/>; null = none.</param>
/// <param name="Label">Small text under the icon (a name, "x5"); blank = none.</param>
/// <param name="Style"><c>style-&lt;name&gt;</c>: replaces the kind's colour; null = the kind's.</param>
public sealed record MinimapMark(
    int Slot,
    string Kind,
    float X,
    float Y,
    float? Yaw = null,
    string? State = null,
    string? Label = null,
    string? Style = null);
