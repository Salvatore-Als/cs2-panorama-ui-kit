using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Roulette;

namespace PanoramaUiKit.Services;

internal sealed class RouletteService(LayoutRegistry registry) : ComponentService<RouletteChannel, RouletteLayout>(registry), IRouletteService
{
    public IUiHandle Spin(CCSPlayerController player, RouletteOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Spin(player, options));

    public void SpinAll(RouletteOptions options) => ForEachHuman(player => Spin(player, options));

    public void Hide(CCSPlayerController player, RouletteLayout? layout = null)
    {
        foreach (RouletteChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(RouletteLayout? layout = null)
    {
        foreach (RouletteChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
