using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Panels;

namespace PanoramaUiKit.Services;

internal sealed class PanelService(LayoutRegistry registry) : ComponentService<PanelChannel, PanelLayout>(registry), IPanelService
{
    public IUiHandle Open(CCSPlayerController player, Func<CCSPlayerController, PanelOptions> build, int page = 0)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return Dead;
        }

        // Built once here to learn which layout it draws on.
        return WhenWarm(Channel(build(player).Layout), player, channel => channel.Open(player, build, live: true, page));
    }

    public IUiHandle Open(CCSPlayerController player, PanelOptions options, int page = 0) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Open(player, _ => options, live: false, page));

    public void Refresh(CCSPlayerController player, PanelLayout? layout = null)
    {
        foreach (PanelChannel channel in Targets(layout))
        {
            channel.Refresh(player);
        }
    }

    public void ShowPage(CCSPlayerController player, int page, PanelLayout? layout = null)
    {
        foreach (PanelChannel channel in Targets(layout))
        {
            channel.ShowPage(player, page);
        }
    }

    public void Close(CCSPlayerController player, PanelLayout? layout = null)
    {
        foreach (PanelChannel channel in Targets(layout))
        {
            channel.Close(player);
        }
    }
}
