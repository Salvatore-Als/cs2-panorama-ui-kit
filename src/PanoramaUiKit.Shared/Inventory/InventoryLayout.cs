using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Inventory;

/// <summary>
/// An inventory layout: a row of <see cref="Slots"/> tiles, <c>inv_0</c>..<c>inv_{Slots-1}</c>, each an item
/// icon with a count and a key hint. Contract "inventory" (docs/CONTRACTS.md).
/// </summary>
public sealed class InventoryLayout : UiLayout
{
    /// <summary>The bar has no entry or exit of its own: it opens and closes at once.</summary>
    public InventoryLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "inventory";

    /// <summary>Tiles in the pool. Slots beyond it are not shown.</summary>
    public int Slots { get; init; } = 6;
}
