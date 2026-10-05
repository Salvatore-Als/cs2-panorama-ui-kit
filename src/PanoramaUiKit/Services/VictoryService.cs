using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Victory;

namespace PanoramaUiKit.Services;

internal sealed class VictoryService(LayoutRegistry registry) : ComponentService<VictoryChannel, VictoryLayout>(registry), IVictoryService
{
    public IUiHandle Show(CCSPlayerController player, VictoryOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    public void ShowAll(VictoryOptions options) => ForEachHuman(player => Show(player, options));

    public void Hide(CCSPlayerController player, VictoryLayout? layout = null)
    {
        foreach (VictoryChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(VictoryLayout? layout = null)
    {
        foreach (VictoryChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
