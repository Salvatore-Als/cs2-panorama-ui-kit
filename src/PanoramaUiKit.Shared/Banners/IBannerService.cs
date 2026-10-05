using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Banners;

/// <summary>
/// One line across the top. A new banner on a player who already has one on the same layout only
/// rewrites text and style - the entry plays once - so it is safe to call every second for a countdown.
/// </summary>
public interface IBannerService
{
    IUiHandle Show(CCSPlayerController player, BannerOptions options);

    /// <summary>Same banner to every connected human.</summary>
    void ShowAll(BannerOptions options);

    /// <summary>Plays the exit for this player. Null layout = on every banner layout.</summary>
    void Hide(CCSPlayerController player, BannerLayout? layout = null);

    void HideAll(BannerLayout? layout = null);
}
