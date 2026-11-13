namespace PanoramaUiKit.Shared.Zone;

/// <summary>
/// Icons the bundled zone stylesheet has a rule for, each a class <c>icon-&lt;name&gt;</c> on <c>zn_status</c>
/// (<see cref="ZoneOptions.Icon"/>) or <c>zn_alert</c> (<see cref="ZoneAlertOptions.Icon"/>).
///
/// <see cref="Ring"/>, <see cref="Closing"/> and <see cref="Danger"/> ship with the addon, white, tinted by the
/// state or the style. <see cref="Slot"/> names a picture the addon does not ship, for another addon to
/// provide, drawn untinted: <c>panorama/images/custom_game/uikit/zone/slots/slot_&lt;n&gt;.png</c> with its
/// <c>slot_&lt;n&gt;_png.vtex</c>. A custom layout can add its own names.
/// </summary>
public static class ZoneIcons
{
    public const string Ring = "ring";
    public const string Closing = "closing";
    public const string Danger = "danger";

    /// <summary>Slots in the bundled stylesheet: <c>slot-1</c> to <c>slot-8</c>.</summary>
    public const int Slots = 8;

    public static readonly IReadOnlyList<string> BuiltIn = [Ring, Closing, Danger];

    /// <summary>Name of picture slot <paramref name="n"/> (1 to <see cref="Slots"/>): <c>slot-&lt;n&gt;</c>.</summary>
    public static string Slot(int n) => $"slot-{n}";

    /// <summary>Class of an icon: <c>icon-&lt;name&gt;</c>.</summary>
    public static string Class(string name) => $"icon-{name}";
}
