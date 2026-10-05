using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Announcements;

/// <summary>
/// Centred card, one per player per layout. A new one while another is up plays the exit first,
/// then the new card enters.
/// </summary>
public interface IAnnounceService
{
    IUiHandle Show(CCSPlayerController player, AnnounceOptions options);

    void ShowAll(AnnounceOptions options);

    /// <summary>Null layout = on every announce layout.</summary>
    void Hide(CCSPlayerController player, AnnounceLayout? layout = null);

    void HideAll(AnnounceLayout? layout = null);
}
