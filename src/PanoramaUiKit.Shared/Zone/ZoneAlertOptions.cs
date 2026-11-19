using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Zone;

/// <summary>
/// The alert in the upper third of the screen, when the zone moves. Gone after <see cref="Seconds"/>.
/// <see cref="UiContent.Style"/> (<see cref="UiStyle.Warn"/> by default, <see cref="UiStyle.Danger"/> for the
/// last zone) and <see cref="UiContent.Classes"/> go on <c>zn_alert</c>; <see cref="UiContent.Texts"/> are
/// written as <c>zn_alert_&lt;key&gt;</c>.
/// </summary>
public sealed record ZoneAlertOptions : UiContent
{
    public ZoneAlertOptions()
    {
        Style = UiStyle.Warn;
    }

    /// <summary>Slot <c>{s:zn_alert_kicker}</c>. Blank collapses it.</summary>
    public string? Kicker { get; init; } = "Warning";

    /// <summary>Slot <c>{s:zn_alert_title}</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Slot <c>{s:zn_alert_desc}</c>. Blank collapses it.</summary>
    public string? Description { get; init; }

    /// <summary><c>icon-&lt;name&gt;</c> on <c>zn_alert</c>: replaces the icon (see <see cref="ZoneIcons"/>). Null = the default.</summary>
    public string? Icon { get; init; }

    /// <summary>Seconds on screen, exit included. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; } = 4f;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required ZoneLayout Layout { get; init; }
}
