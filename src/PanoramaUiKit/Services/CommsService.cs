using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Comms;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Services;

internal sealed class CommsService(LayoutRegistry registry) : ComponentService<CommsChannel, CommsLayout>(registry), ICommsService
{
    /// <summary>The bundled faces, for the map's precache, so a face is not loaded on first use.</summary>
    public static IEnumerable<string> Resources()
    {
        foreach (string name in CommsFaces.All)
        {
            // Photo faces are PNGs (a .vtex), the others white SVGs.
            yield return name == CommsFaces.Villain
                ? $"panorama/images/custom_game/uikit/comms/{name}_png.vtex"
                : $"panorama/images/custom_game/uikit/comms/{name}.vsvg";
        }
    }

    public IUiHandle Show(CCSPlayerController player, CommsOptions options)
    {
        string? sound = options.Sound?.Trim();
        return WhenWarm(Channel(options.Layout), player, channel =>
        {
            IUiHandle handle = channel.Show(player, options);
            if (handle.IsVisible && !string.IsNullOrEmpty(sound))
            {
                player.ExecuteClientCommand($"play {sound}");
            }

            return handle;
        });
    }

    public void ShowAll(CommsOptions options) => ForEachHuman(player => Show(player, options));

    public bool SetText(CCSPlayerController player, string text, CommsLayout? layout = null)
    {
        bool any = false;
        foreach (CommsChannel channel in Targets(layout))
        {
            any |= channel.SetText(player, text);
        }

        return any;
    }

    public void Hide(CCSPlayerController player, CommsLayout? layout = null)
    {
        foreach (CommsChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(CommsLayout? layout = null)
    {
        foreach (CommsChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
