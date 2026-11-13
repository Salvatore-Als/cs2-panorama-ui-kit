using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Inventory;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int InventoryDemoAmmo = 120;
    private const int InventoryDemoShot = 30;

    /// <summary>What the inventory demo shows each caller: changed a little on every call.</summary>
    private sealed class InventoryDemoState
    {
        public int Selected;
        public bool FlashlightOn = true;
        public int Ammo = InventoryDemoAmmo;
    }

    private readonly Dictionary<int, InventoryDemoState> _inventories = new();

    /// <summary>
    /// A six-tile inventory. Every call moves the selection one tile right, toggles the flashlight and fires
    /// a magazine (the ammo tile pulses, then reaches 0 and goes depleted). <c>select n</c> picks a tile,
    /// <c>off</c> hides it.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_inventory", "Inventory bar: each call changes the selection, the flashlight and the ammo.")]
    [CommandHelper(usage: "[select <n> | off]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoInventory(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        string verb = command.ArgCount > 1 ? command.GetArg(1).ToLowerInvariant() : string.Empty;
        if (verb == "off")
        {
            _inventories.Remove(target.Slot);
            kit.Inventory.Hide(target, DemoLayouts.Inventory);
            return;
        }

        if (!_inventories.TryGetValue(target.Slot, out InventoryDemoState? state)
            || !kit.Inventory.IsShowing(target, DemoLayouts.Inventory))
        {
            state = new InventoryDemoState();
            _inventories[target.Slot] = state;
        }
        else if (verb != "select")
        {
            state.Selected = (state.Selected + 1) % 6;
            state.FlashlightOn = !state.FlashlightOn;
            state.Ammo = Math.Max(0, state.Ammo - InventoryDemoShot);
        }

        if (verb == "select" && command.ArgCount > 2 && int.TryParse(command.GetArg(2), out int asked))
        {
            state.Selected = Math.Clamp(asked - 1, 0, 5);
        }

        command.ReplyToCommand($"[uikit] inventory: slot {state.Selected + 1}, flashlight {(state.FlashlightOn ? "on" : "off")}, ammo {state.Ammo}");
        kit.Inventory.Show(target, new InventoryOptions
        {
            Layout = DemoLayouts.Inventory,
            Selected = state.Selected,
            Style = UiStyle.Gold,
            Slots =
            [
                new InventorySlot(state.FlashlightOn ? InventoryItems.FlashlightOn : InventoryItems.FlashlightOff,
                    Name: state.FlashlightOn ? "Flashlight (on)" : "Flashlight (off)", Style: state.FlashlightOn ? UiStyle.Warn : UiStyle.Neutral),
                new InventorySlot(InventoryItems.Ammo, state.Ammo, Name: "Rifle ammo"),
                new InventorySlot(InventoryItems.Medkit, 2, Name: "Medkit", Style: UiStyle.Danger),
                new InventorySlot(InventoryItems.Bandage, 5, Name: "Bandage"),
                new InventorySlot(InventoryItems.Grenade, 1, Name: "Grenade", Style: UiStyle.Success),
                new InventorySlot(InventoryItems.Battery, 0, Name: "Battery", Style: UiStyle.Info),
            ],
        });
    }
}
