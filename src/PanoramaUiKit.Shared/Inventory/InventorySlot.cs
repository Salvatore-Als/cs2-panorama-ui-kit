using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Inventory;

/// <summary>
/// One tile of the inventory.
/// <list type="bullet">
/// <item><see cref="Item"/>: the icon, applied as <c>item-&lt;name&gt;</c> on <c>inv_&lt;n&gt;_icon</c>. See <see cref="InventoryItems"/>.</item>
/// <item><see cref="Count"/>: slot <c>{s:inv_&lt;n&gt;_count}</c>. Null shows no number (a flashlight); 0 adds <c>depleted</c>.</item>
/// <item><see cref="Key"/>: slot <c>{s:inv_&lt;n&gt;_key}</c>, the key hint ("1", "F"). Blank collapses it.</item>
/// <item><see cref="Name"/>: slot <c>{s:inv_&lt;n&gt;_name}</c>, shown under the tile while it is selected. Blank collapses it.</item>
/// <item><see cref="Style"/>: <c>style-*</c> on <c>inv_&lt;n&gt;</c>, the tile's accent and icon tint.</item>
/// </list>
/// </summary>
public sealed record InventorySlot(string Item, int? Count = null, string? Key = null, string? Name = null, string Style = UiStyle.Neutral);
