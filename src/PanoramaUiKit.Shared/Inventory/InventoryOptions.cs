using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Inventory;

/// <summary>
/// The inventory bar of one player. <see cref="UiContent.Style"/> (the colour of the selected tile) and
/// <see cref="UiContent.Classes"/> go on <c>inv_bar</c>; <see cref="UiContent.Texts"/> are written as
/// <c>inv_&lt;key&gt;</c>.
/// </summary>
public sealed record InventoryOptions : UiContent
{
    /// <summary>Tiles, left to right. Only the first layout <c>Slots</c> are shown. Empty hides the bar.</summary>
    public IReadOnlyList<InventorySlot> Slots { get; init; } = [];

    /// <summary>Index of the selected tile (<c>selected</c>, its name shown), null for none.</summary>
    public int? Selected { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required InventoryLayout Layout { get; init; }
}
