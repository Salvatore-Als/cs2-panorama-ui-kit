using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Toasts;

namespace PanoramaUiKit.Services;

internal sealed class ToastService(LayoutRegistry registry) : ComponentService<ToastChannel, ToastLayout>(registry), IToastService
{
    public IUiHandle Show(CCSPlayerController player, ToastOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    public void ShowAll(ToastOptions options) => ForEachHuman(player => Show(player, options));

    public void Clear(CCSPlayerController player, ToastLayout? layout = null)
    {
        foreach (ToastChannel channel in Targets(layout))
        {
            channel.ClearAll(player);
        }
    }
}
