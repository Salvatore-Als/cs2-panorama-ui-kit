namespace PanoramaUiKit.Shared.Compass;

/// <summary>
/// Pin shapes the bundled stylesheet has a rule for, each a class <c>kind-&lt;name&gt;</c> on the pin
/// (picture <c>panorama/images/custom_game/uikit/compass/&lt;name&gt;.vsvg</c>). A custom layout can add its own.
/// Also the conversions between the game's yaw and a compass heading.
/// </summary>
public static class CompassNames
{
    public const string Dot = "dot";
    public const string Diamond = "diamond";
    public const string Zone = "zone";
    public const string Skull = "skull";
    public const string Flag = "flag";
    public const string Crate = "crate";

    public static readonly IReadOnlyList<string> All = [Dot, Diamond, Zone, Skull, Flag, Crate];

    /// <summary>Class of a shape on <c>cp_pin_&lt;n&gt;</c>: <c>kind-&lt;name&gt;</c>.</summary>
    public static string Class(string name) => $"kind-{name}";

    /// <summary>
    /// Compass heading of a yaw, 0 to 360: north (0) is +Y, east (90) is +X. The game's yaw turns the other
    /// way and starts on +X, so a yaw of 90 is north and a yaw of 0 is east.
    /// </summary>
    public static float HeadingFromYaw(float yawDegrees) => Wrap360(90f - yawDegrees);

    /// <summary>Heading from one world point to another, on the ground plane.</summary>
    public static float HeadingTo(float fromX, float fromY, float toX, float toY)
    {
        float yaw = MathF.Atan2(toY - fromY, toX - fromX) * (180f / MathF.PI);
        return HeadingFromYaw(yaw);
    }

    /// <summary>Any angle into 0 (inclusive) to 360 (exclusive).</summary>
    public static float Wrap360(float degrees)
    {
        float wrapped = degrees % 360f;
        return wrapped < 0f ? wrapped + 360f : wrapped;
    }
}
