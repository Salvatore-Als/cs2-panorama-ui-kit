using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Compass;

namespace PanoramaUiKit.Services;

internal sealed class CompassService(LayoutRegistry registry) : ComponentService<CompassChannel, CompassLayout>(registry), ICompassService
{
    public IUiHandle Show(CCSPlayerController player, CompassOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    public bool SetPins(CCSPlayerController player, IReadOnlyList<CompassPin> pins, CompassLayout? layout = null)
    {
        if (player is not { IsValid: true })
        {
            return false;
        }

        bool any = false;
        foreach (CompassChannel channel in Targets(layout))
        {
            any |= channel.SetPins(player, pins);
        }

        return any;
    }

    public bool IsShowing(CCSPlayerController player, CompassLayout? layout = null)
    {
        return player is { IsValid: true } && Targets(layout).Any(channel => channel.IsShowing(player));
    }

    public void Hide(CCSPlayerController player, CompassLayout? layout = null)
    {
        foreach (CompassChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(CompassLayout? layout = null)
    {
        foreach (CompassChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
