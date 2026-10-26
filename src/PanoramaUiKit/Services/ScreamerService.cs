using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Screamers;

namespace PanoramaUiKit.Services;

internal sealed class ScreamerService(LayoutRegistry registry) : ComponentService<ScreamerChannel, ScreamerLayout>(registry), IScreamerService
{
    /// <summary>
    /// The bundled sound and pictures, for the map's precache, so neither is loaded on first use.
    /// </summary>
    public static IEnumerable<string> Resources()
    {
        yield return ScreamerNames.Sound;
        foreach (string name in ScreamerNames.All)
        {
            yield return $"panorama/images/custom_game/uikit/screamer/{name}_png.vtex";
        }
    }

    public IUiHandle Show(CCSPlayerController player, ScreamerOptions options)
    {
        ScreamerChannel? channel = Channel(options.Layout);

        // No screamer named, nothing on screen.
        string? name = Names.Token(options.Name);
        if (name == null)
        {
            channel?.CancelPending(player.Slot);
            channel?.Hide(player, ScreamerChannel.Key);
            return Dead;
        }

        ScreamerContent content = new(name, options.Seconds, options);
        string? sound = options.PlaySound ? options.Sound?.Trim() : null;
        return WhenWarm(channel, player, screamer =>
        {
            bool fresh = screamer.ShowingName(player) != name;
            if (!fresh || string.IsNullOrEmpty(sound) || player is not { IsValid: true, IsBot: false })
            {
                return screamer.Show(player, ScreamerChannel.Key, content);
            }

            // From the server, not CSS `sound`: a stylesheet sound loads on use and starts after the picture.
            player.ExecuteClientCommand($"play {sound}");
            if (options.SoundLeadSeconds <= 0f)
            {
                return screamer.Show(player, ScreamerChannel.Key, content);
            }

            // The client is slow to start a sound: the picture waits for it.
            UiHandle handle = new(player);
            screamer.ShowLater(player, options.SoundLeadSeconds, content, handle);
            return handle;
        });
    }

    public bool IsShowing(CCSPlayerController player, ScreamerLayout? layout = null)
    {
        return player is { IsValid: true } && Targets(layout).Any(channel => channel.IsShowing(player));
    }

    public void Hide(CCSPlayerController player, ScreamerLayout? layout = null)
    {
        foreach (ScreamerChannel channel in Targets(layout))
        {
            if (player is { IsValid: true })
            {
                channel.CancelPending(player.Slot);
            }

            channel.HideAll(player);
        }
    }

    public void HideAll(ScreamerLayout? layout = null)
    {
        foreach (ScreamerChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
