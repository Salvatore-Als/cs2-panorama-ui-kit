using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Recap;

namespace PanoramaUiKit.Services;

internal sealed class RecapService(LayoutRegistry registry) : ComponentService<RecapChannel, RecapLayout>(registry), IRecapService
{
    public IUiHandle Show(CCSPlayerController player, RecapOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    // Player left null: each viewer's own name is filled in by the channel.
    public void ShowAll(RecapOptions options) => ForEachHuman(player => Show(player, options));

    public void UpdateStat(CCSPlayerController player, int index, string value, RecapLayout? layout = null)
    {
        if (player is not { IsValid: true } || index < 0)
        {
            return;
        }

        foreach (RecapChannel channel in Targets(layout))
        {
            channel.UpdateStat(player, index, value ?? string.Empty);
        }
    }

    public void Hide(CCSPlayerController player, RecapLayout? layout = null)
    {
        foreach (RecapChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(RecapLayout? layout = null)
    {
        foreach (RecapChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
