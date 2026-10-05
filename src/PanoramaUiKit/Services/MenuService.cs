using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Menus;

namespace PanoramaUiKit.Services;

internal sealed class MenuService(LayoutRegistry registry) : ComponentService<MenuChannel, MenuLayout>(registry), IMenuService
{
    public IUiHandle Open(CCSPlayerController player, MenuOptions menu) =>
        WhenWarm(Channel(menu.Layout), player, channel => channel.Open(player, menu));

    /// <summary>A sub-menu stays on the layout its root was opened on.</summary>
    public void Push(CCSPlayerController player, MenuOptions menu)
    {
        if (OpenOn(player) is MenuChannel channel)
        {
            channel.Push(player, menu);
            return;
        }

        Open(player, menu);
    }

    public void Back(CCSPlayerController player) => OpenOn(player)?.Back(player);

    public void Close(CCSPlayerController player, MenuLayout? layout = null)
    {
        foreach (MenuChannel channel in Targets(layout))
        {
            channel.Close(player);
        }
    }

    private MenuChannel? OpenOn(CCSPlayerController player) => Targets(null).FirstOrDefault(c => c.IsOpenFor(player));
}
