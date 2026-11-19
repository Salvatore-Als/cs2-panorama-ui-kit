using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Inventory;

namespace PanoramaUiKit.Services;

internal sealed class InventoryService(LayoutRegistry registry) : ComponentService<InventoryChannel, InventoryLayout>(registry), IInventoryService
{
    public IUiHandle Show(CCSPlayerController player, InventoryOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    public void SetCount(CCSPlayerController player, int index, int? count, InventoryLayout? layout = null)
    {
        foreach (InventoryChannel channel in Targets(layout))
        {
            channel.SetCount(player, index, count);
        }
    }

    public void Select(CCSPlayerController player, int? index, InventoryLayout? layout = null)
    {
        foreach (InventoryChannel channel in Targets(layout))
        {
            channel.Select(player, index);
        }
    }

    public bool IsShowing(CCSPlayerController player, InventoryLayout? layout = null)
    {
        return player is { IsValid: true } && Targets(layout).Any(channel => channel.IsShowing(player));
    }

    public void Hide(CCSPlayerController player, InventoryLayout? layout = null)
    {
        foreach (InventoryChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(InventoryLayout? layout = null)
    {
        foreach (InventoryChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }
}
