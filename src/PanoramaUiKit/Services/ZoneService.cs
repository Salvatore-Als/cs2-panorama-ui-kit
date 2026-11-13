using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Zone;

namespace PanoramaUiKit.Services;

internal sealed class ZoneService(LayoutRegistry registry) : ComponentService<ZoneChannel, ZoneLayout>(registry), IZoneService
{
    public IUiHandle Show(CCSPlayerController player, ZoneOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    public void ShowAll(ZoneOptions options) => ForEachHuman(player => Show(player, options));

    public void SetOutside(CCSPlayerController player, bool outside, ZoneLayout? layout = null)
    {
        foreach (ZoneChannel channel in Targets(layout))
        {
            channel.SetOutside(player, outside);
        }
    }

    public IUiHandle Alert(CCSPlayerController player, ZoneAlertOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Alert(player, options));

    public void AlertAll(ZoneAlertOptions options) => ForEachHuman(player => Alert(player, options));

    public void Hide(CCSPlayerController player, ZoneLayout? layout = null)
    {
        foreach (ZoneChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(ZoneLayout? layout = null)
    {
        foreach (ZoneChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
