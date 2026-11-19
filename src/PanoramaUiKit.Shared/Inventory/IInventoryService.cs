using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Inventory;

/// <summary>
/// A row of item tiles, per player: icon, count, key hint, one selected. <see cref="Show"/> on a bar that is
/// up repaints it in place and sends only what changed, so it can be called on every change. A count that
/// changes pulses its tile. <see cref="SetCount"/> and <see cref="Select"/> change one thing of the bar on
/// screen without passing the whole list again.
/// </summary>
public interface IInventoryService
{
    /// <summary>Shows or repaints the bar. Empty <see cref="InventoryOptions.Slots"/> hides it.</summary>
    IUiHandle Show(CCSPlayerController player, InventoryOptions options);

    /// <summary>
    /// New count for the tile at <paramref name="index"/> of the bar on screen. Null removes the number.
    /// Nothing happens when no bar is up or the index is out of range. Null layout = every inventory layout.
    /// </summary>
    void SetCount(CCSPlayerController player, int index, int? count, InventoryLayout? layout = null);

    /// <summary>Selects the tile at <paramref name="index"/> of the bar on screen; null selects none.</summary>
    void Select(CCSPlayerController player, int? index, InventoryLayout? layout = null);

    /// <summary>True while a bar is on this player's screen, on that layout or on any when null.</summary>
    bool IsShowing(CCSPlayerController player, InventoryLayout? layout = null);

    /// <summary>Takes it off this player's screen. Null layout = on every inventory layout.</summary>
    void Hide(CCSPlayerController player, InventoryLayout? layout = null);

    void HideAll(InventoryLayout? layout = null);
}
