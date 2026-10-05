using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Votes;

namespace PanoramaUiKit.Services;

internal sealed class VoteService(LayoutRegistry registry) : ComponentService<VoteChannel, VoteLayout>(registry), IVoteService
{
    public IUiHandle Show(CCSPlayerController player, VoteOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    public void ShowAll(VoteOptions options) => ForEachHuman(player => Show(player, options));

    public void Update(CCSPlayerController player, VoteOptions options) =>
        WhenWarm(Channel(options.Layout), channel => channel.Update(player, options));

    public void UpdateAll(VoteOptions options) => ForEachHuman(player => Update(player, options));

    public void Hide(CCSPlayerController player, VoteLayout? layout = null)
    {
        foreach (VoteChannel channel in Targets(layout))
        {
            channel.Close(player);
        }
    }

    public void HideAll(VoteLayout? layout = null)
    {
        foreach (VoteChannel channel in Targets(layout))
        {
            channel.CloseEveryone();
        }
    }
}
