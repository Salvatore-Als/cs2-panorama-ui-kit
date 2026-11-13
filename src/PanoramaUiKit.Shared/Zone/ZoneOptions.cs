using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Zone;

/// <summary>
/// The status widget, under the compass. Stays until hidden: send it again for the next phase.
/// <see cref="UiContent.Style"/> and <see cref="UiContent.Classes"/> go on <c>zn_status</c>;
/// <see cref="UiContent.Texts"/> are written as <c>zn_&lt;key&gt;</c>.
/// </summary>
public sealed record ZoneOptions : UiContent
{
    /// <summary>
    /// <c>at-*</c> on <c>zn_status</c>: a corner, or under the compass placed at the same name (above it at the
    /// bottom). See <see cref="ZonePlacement"/>.
    /// </summary>
    public ZonePlacement Placement { get; init; } = ZonePlacement.UnderHud;

    /// <summary>Slot <c>{s:zn_phase}</c>: "Zone 2". Blank collapses it.</summary>
    public string? Phase { get; init; }

    /// <summary><c>state-*</c> on <c>zn_status</c>, and the default of <see cref="StateText"/>.</summary>
    public ZoneState State { get; init; } = ZoneState.Waiting;

    /// <summary>Slot <c>{s:zn_state}</c>. Null = the state's own text ("Closing in", "Closing", "Final zone").</summary>
    public string? StateText { get; init; }

    /// <summary>
    /// Seconds left on the countdown <c>{s:zn_timer}</c>, ticked by the kit every second. It stops at zero and
    /// stays there: the plugin sends the next phase. Zero or less = no countdown (collapsed).
    /// </summary>
    public float Seconds { get; init; }

    /// <summary>
    /// Length of the whole phase, for the bar: it starts at <see cref="Seconds"/> / TotalSeconds and drains to
    /// empty with the countdown, run by the client. Zero or less hides the bar.
    /// </summary>
    public float TotalSeconds { get; init; }

    /// <summary><c>string.Format</c> pattern of the countdown: <c>{0}</c> = minutes, <c>{1}</c> = seconds.</summary>
    public string TimerFormat { get; init; } = "{0}:{1:00}";

    /// <summary>Slot <c>{s:zn_info}</c>: "Safe zone 240 m", "12 alive". Blank collapses it.</summary>
    public string? Info { get; init; }

    /// <summary>
    /// <c>icon-&lt;name&gt;</c> on <c>zn_status</c>: replaces the state's icon (see <see cref="ZoneIcons"/>, including
    /// pictures another addon provides). Null = the state's own. Outside the zone the warning glyph still wins.
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>The player is outside the safe zone: <c>outside</c> on <c>zn_status</c>. See also <see cref="IZoneService.SetOutside"/>.</summary>
    public bool Outside { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required ZoneLayout Layout { get; init; }
}
