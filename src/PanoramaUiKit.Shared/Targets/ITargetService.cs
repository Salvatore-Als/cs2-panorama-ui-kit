using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Targets;

/// <summary>
/// Text next to the crosshair: "Press E to open", the name of what is aimed at. One per position and
/// player; showing again at the same position rewrites it in place.
/// </summary>
public interface ITargetService
{
    IUiHandle Show(CCSPlayerController player, TargetOptions options);

    /// <summary>Same text to every connected human.</summary>
    void ShowAll(TargetOptions options);

    /// <summary>Hides this player's text at that position, or at every position when null. Null layout = on every target layout.</summary>
    void Hide(CCSPlayerController player, TargetPosition? position = null, TargetLayout? layout = null);

    void HideAll(TargetPosition? position = null, TargetLayout? layout = null);
}
