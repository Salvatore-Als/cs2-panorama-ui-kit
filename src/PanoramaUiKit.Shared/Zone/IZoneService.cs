using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Zone;

/// <summary>
/// The safe zone: a status widget under the compass (phase, countdown ticked by the kit, a bar drained by the
/// client) and an alert when it moves. Per player, so each one can be told whether they are outside.
/// Both live on one layout; the layout closes once neither is up.
/// </summary>
public interface IZoneService
{
    /// <summary>Shows or repaints the status. Sent again with the same end time, the bar keeps running.</summary>
    IUiHandle Show(CCSPlayerController player, ZoneOptions options);

    void ShowAll(ZoneOptions options);

    /// <summary>Turns <c>outside</c> on the status on or off, without touching the rest. Null layout = every zone layout.</summary>
    void SetOutside(CCSPlayerController player, bool outside, ZoneLayout? layout = null);

    /// <summary>Shows the alert, with or without the status. A new alert while one is up replaces its text.</summary>
    IUiHandle Alert(CCSPlayerController player, ZoneAlertOptions options);

    void AlertAll(ZoneAlertOptions options);

    /// <summary>Takes the status and the alert off. Null layout = every zone layout.</summary>
    void Hide(CCSPlayerController player, ZoneLayout? layout = null);

    void HideAll(ZoneLayout? layout = null);
}
