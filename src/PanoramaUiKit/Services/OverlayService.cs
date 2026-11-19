using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Overlays;

namespace PanoramaUiKit.Services;

internal sealed class OverlayService(LayoutRegistry registry) : ComponentService<OverlayChannel, OverlayLayout>(registry), IOverlayService
{
    public IUiHandle Show(CCSPlayerController player, OverlayOptions options)
    {
        OverlayChannel? channel = Channel(options.Layout);

        // No effect named, nothing on screen.
        string? effect = Names.Token(options.Effect);
        if (effect == null)
        {
            channel?.Hide(player, OverlayChannel.Key);
            return Dead;
        }

        OverlayContent content = new(effect, options.Seconds, options.KeepOnDeath, options);
        return WhenWarm(channel, player, overlay => overlay.Show(player, OverlayChannel.Key, content));
    }

    public void ShowAll(OverlayOptions options) => ForEachHuman(player => Show(player, options));

    public bool IsShowing(CCSPlayerController player, OverlayLayout? layout = null) =>
        player is { IsValid: true } && Targets(layout).Any(channel => channel.IsShowing(player));

    public void Hide(CCSPlayerController player, OverlayLayout? layout = null)
    {
        foreach (OverlayChannel channel in Targets(layout))
        {
            channel.HideAll(player);
        }
    }

    public void HideAll(OverlayLayout? layout = null)
    {
        foreach (OverlayChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }

    /// <summary>A player died: on every overlay layout, an effect that does not outlive a death goes.</summary>
    public void PlayerDied(CCSPlayerController player)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return;
        }

        foreach (OverlayChannel channel in Targets(null))
        {
            channel.PlayerDied(player);
        }
    }
}
