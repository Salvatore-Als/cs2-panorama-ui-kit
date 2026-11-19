using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Inventory;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Inventory bar on one layout (contract "inventory"), per viewer: a pool of tiles <c>inv_&lt;n&gt;</c>.
///
/// Every change repaints the whole bar through the viewer's <see cref="ClientState"/>, so only what
/// changed is sent and a steady bar costs nothing. A count that changes pulses its tile by swapping
/// <c>pulse-a</c> and <c>pulse-b</c>: the same class off then on in one tick is no diff and never reaches
/// the client, two names always do.
/// </summary>
internal sealed class InventoryChannel : LayoutChannel<InventoryChannel.Viewer>
{
    private const string BarId = "inv_bar";

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client, int tiles) : base(client)
        {
            Painted = new (string Item, int? Count)?[tiles];
            Pulse = new bool[tiles];
        }

        /// <summary>What is on screen now; null until the first paint.</summary>
        public InventoryOptions? Options { get; set; }

        /// <summary>Item and count each tile was painted with, null when unused: a count change pulses.</summary>
        public (string Item, int? Count)?[] Painted { get; }

        /// <summary>Which of the two pulse classes each tile is on: the next pulse is the other one.</summary>
        public bool[] Pulse { get; }
    }

    private readonly InventoryLayout _layout;

    public InventoryChannel(BasePlugin plugin, InventoryLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient(), _layout.Slots);

    public bool IsShowing(CCSPlayerController player) => Viewers.TryGetValue(player.Slot, out Viewer? viewer) && viewer.Options != null;

    public IUiHandle Show(CCSPlayerController player, InventoryOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        if (options.Slots.Count == 0)
        {
            ClearAll(player);
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out _);
        Paint(player, viewer, options);
        if (viewer.Handle is { IsVisible: true } current)
        {
            return current;
        }

        return viewer.NewHandle(player, () => Hide(player));
    }

    /// <summary>The bar on screen with one tile's count changed. False when there is no such tile.</summary>
    public bool SetCount(CCSPlayerController player, int index, int? count)
    {
        if (!TryCurrent(player, out Viewer viewer, out InventoryOptions options) || index < 0 || index >= options.Slots.Count)
        {
            return false;
        }

        List<InventorySlot> slots = options.Slots.ToList();
        slots[index] = slots[index] with { Count = count };
        Paint(player, viewer, options with { Slots = slots });
        return true;
    }

    /// <summary>The bar on screen with another tile selected, or none.</summary>
    public bool Select(CCSPlayerController player, int? index)
    {
        if (!TryCurrent(player, out Viewer viewer, out InventoryOptions options))
        {
            return false;
        }

        Paint(player, viewer, options with { Selected = index });
        return true;
    }

    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.ContainsKey(player.Slot))
        {
            ClearAll(player);
        }
    }

    public void HideEveryone() => ClearEveryone();

    private bool TryCurrent(CCSPlayerController player, out Viewer viewer, out InventoryOptions options)
    {
        options = null!;
        if (player is not { IsValid: true } || !TryOpenViewer(player, out viewer) || viewer.Options == null)
        {
            viewer = null!;
            return false;
        }

        options = viewer.Options;
        return true;
    }

    private void Paint(CCSPlayerController player, Viewer viewer, InventoryOptions options)
    {
        ClientState client = viewer.Client;
        viewer.Options = options;

        client.Group(player, BarId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, BarId, options.Classes);
        client.ExtraTexts(player, "inv", options.Texts);

        int count = Math.Min(options.Slots.Count, _layout.Slots);
        for (int i = 0; i < _layout.Slots; i++)
        {
            string id = $"inv_{i}";
            if (i >= count)
            {
                viewer.Painted[i] = null;
                client.Class(player, id, "hidden", true);
                continue;
            }

            InventorySlot slot = options.Slots[i];
            string item = Names.Token(slot.Item) ?? string.Empty;

            client.Group(player, $"{id}_icon", "item", item.Length > 0 ? InventoryItems.Class(item) : null);
            client.OptionalText(player, $"{id}_count", slot.Count?.ToString());
            client.OptionalText(player, $"{id}_key", slot.Key);
            client.OptionalText(player, $"{id}_name", slot.Name);
            client.Group(player, id, "style", Names.StyleOrNeutral(slot.Style));
            client.Class(player, id, "selected", options.Selected == i);
            client.Class(player, id, "depleted", slot.Count == 0);

            // Same item, another count: the tile pulses. A new item or a first paint does not.
            if (viewer.Painted[i] is { } before && before.Item == item && before.Count != slot.Count)
            {
                viewer.Pulse[i] = !viewer.Pulse[i];
                client.Group(player, id, "pulse", viewer.Pulse[i] ? "pulse-a" : "pulse-b");
            }

            viewer.Painted[i] = (item, slot.Count);
            client.Class(player, id, "hidden", false);
        }
    }

    /// <summary>After a round restart the layout comes back scrubbed: the bar is painted again.</summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored
            && Viewers.TryGetValue(e.Player.Slot, out Viewer? viewer)
            && viewer.Options is { } options)
        {
            Forget(e.Player.Slot);
            Show(e.Player, options);
            return;
        }

        base.OnPanelEvent(e);
    }
}
