using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Victory;

/// <summary>
/// End of round screen. Per player, so each side can read its own result; <see cref="ShowAll"/>
/// for one screen for everyone.
/// </summary>
public interface IVictoryService
{
    IUiHandle Show(CCSPlayerController player, VictoryOptions options);

    void ShowAll(VictoryOptions options);

    /// <summary>Null layout = every victory layout.</summary>
    void Hide(CCSPlayerController player, VictoryLayout? layout = null);

    void HideAll(VictoryLayout? layout = null);
}
