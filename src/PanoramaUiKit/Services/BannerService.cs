using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Services;

internal sealed class BannerService(LayoutRegistry registry) : ComponentService<CardChannel, BannerLayout>(registry), IBannerService
{
    public IUiHandle Show(CCSPlayerController player, BannerOptions options)
    {
        CardChannel? channel = Channel(options.Layout);

        // Nothing to say, nothing on screen.
        if (string.IsNullOrWhiteSpace(options.Text))
        {
            channel?.Hide(player);
            return Dead;
        }

        CardContent content = new(options.Text, null, options.Kicker, options.Seconds, options);
        return WhenWarm(channel, player, card => card.Show(player, content));
    }

    public void ShowAll(BannerOptions options) => ForEachHuman(player => Show(player, options));

    public void Hide(CCSPlayerController player, BannerLayout? layout = null)
    {
        foreach (CardChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(BannerLayout? layout = null)
    {
        foreach (CardChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
