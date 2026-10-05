using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Menus;

/// <summary>
/// Menus and sub-menus. A <see cref="MenuEntryKind.Submenu"/> entry pushes its menu on top;
/// the back button (or <see cref="Back"/>) returns to the parent on the page it was left on.
/// Entries are paginated by the layout's pool size. Takes the mouse while open.
/// </summary>
public interface IMenuService
{
    /// <summary>Opens <paramref name="menu"/> as the root, replacing whatever menu the player had on that layout.</summary>
    IUiHandle Open(CCSPlayerController player, MenuOptions menu);

    /// <summary>Opens <paramref name="menu"/> on top of the current one, with a back button. Opens it as root if none is open.</summary>
    void Push(CCSPlayerController player, MenuOptions menu);

    /// <summary>Returns to the parent menu. On the root menu, closes.</summary>
    void Back(CCSPlayerController player);

    /// <summary>Null layout = every menu layout.</summary>
    void Close(CCSPlayerController player, MenuLayout? layout = null);
}
