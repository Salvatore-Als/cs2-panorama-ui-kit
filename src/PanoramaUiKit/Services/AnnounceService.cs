using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Services;

internal sealed class AnnounceService(LayoutRegistry registry) : ComponentService<CardChannel, AnnounceLayout>(registry), IAnnounceService
{
    private static readonly string[] AnimateTitle = ["animate-title"];

    public IUiHandle Show(CCSPlayerController player, AnnounceOptions options)
    {
        IReadOnlyList<string>? flags = null;
        if (options.AnimateTitle)
        {
            flags = AnimateTitle;
        }

        CardContent content = new(options.Title, options.Description, options.Kicker, options.Seconds, options, flags);
        return WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, content));
    }

    public void ShowAll(AnnounceOptions options) => ForEachHuman(player => Show(player, options));

    public void Hide(CCSPlayerController player, AnnounceLayout? layout = null)
    {
        foreach (CardChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(AnnounceLayout? layout = null)
    {
        foreach (CardChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
