# Inventory

A row of item tiles at the bottom centre of one player's screen: an icon, a count, a key hint, one tile selected. For a flashlight, ammo, medkits, anything the player carries.

The icon is one class of the stylesheet, `item-<name>`, whose `background-image` is a white SVG tinted by the tile's style. A count that changes pulses its tile; a count of 0 greys the icon out.

Bundled layout: `uikit_inventory` (6 tiles). Demo command: `css_uikit_demo_inventory [select <n> | off]`. Each call moves the selection, toggles the flashlight and fires a magazine.

## Declare the layout

```csharp
static readonly InventoryLayout Inventory = new() { Name = "uikit_inventory" };
```

## Show the bar

```csharp
IUiHandle handle = kit.Inventory.Show(player, new InventoryOptions
{
    Layout = Inventory,
    Selected = 1,
    Style = UiStyle.Gold,
    Slots =
    [
        new InventorySlot(InventoryItems.FlashlightOn, Key: "1", Name: "Flashlight", Style: UiStyle.Warn),
        new InventorySlot(InventoryItems.Ammo, 120, "2", "Rifle ammo"),
        new InventorySlot(InventoryItems.Medkit, 2, "3", "Medkit", UiStyle.Danger),
        new InventorySlot(InventoryItems.Battery, 0, "4", "Battery"),
    ],
});
```

| Option | Default | Meaning |
|---|---|---|
| `Slots` | empty | Tiles, left to right. Only the first `InventoryLayout.Slots` are shown. Empty hides the bar. |
| `Selected` | `null` | Index of the selected tile: lifted, outlined, its name shown under it. `null` = none. |
| `Style` | `neutral` | Colour of the selected tile's outline, `style-*` on `inv_bar`. |

Per tile, `InventorySlot`:

| Field | Default | Meaning |
|---|---|---|
| `Item` | required | Icon, applied as `item-<name>` on `inv_<n>_icon`. See `InventoryItems`. |
| `Count` | `null` | Number at the bottom right. `null` shows none (a flashlight). `0` adds `depleted`. |
| `Key` | `null` | Key hint at the top left (`"1"`, `"F"`). Blank hides it. |
| `Name` | `null` | Shown under the tile while it is selected. Blank shows nothing. |
| `Style` | `neutral` | Accent line and icon tint. Neutral keeps the icon white. |

`Show` on a bar that is up repaints it in place and sends only what changed: call it on every change. A tile whose count changes, same item, pulses.

## Change one thing

```csharp
kit.Inventory.SetCount(player, 1, 90);         // tile 1 now shows 90, and pulses
kit.Inventory.SetCount(player, 0, null);       // tile 0 shows no number
kit.Inventory.Select(player, 2);               // tile 2 selected
kit.Inventory.Select(player, null);            // none selected
```

Both work on the bar on screen and do nothing when there is none. With no layout they apply to every inventory layout.

To switch the flashlight, show the bar again with `FlashlightOff` in that tile: a new item is drawn without a pulse.

## The bundled items

`InventoryItems.All`, in `workshop/panorama/images/custom_game/uikit/items/`:

| Name | Icon |
|---|---|
| `flashlight_on` | Torch with its beam |
| `flashlight_off` | Torch, no beam |
| `ammo` | Three cartridges |
| `medkit` | Case with a cross |
| `bandage` | Strip with a pad |
| `armor` | Shield |
| `grenade` | Grenade with its ring |
| `battery` | Battery with charge bars |
| `key` | Key |
| `food` | Ration can |

Each is a 64x64 single-colour white SVG: the stylesheet tints it with `wash-color`. The kit addon compiles `<name>.svg` to `<name>.vsvg_c`.

**Unverified in game:** a `.vsvg` from the addon as a `background-image`. Valve's own layouts use `.vsvg` as an `Image` source. If an icon draws nothing, use a PNG with its `.vtex` instead, as the screamer pictures do.

## Add an item

Draw a white SVG (viewBox `0 0 64 64`, plain paths, no gradients, filters or text), put it in the addon at `panorama/images/custom_game/uikit/items/<name>.svg`, and add a rule to `uikit_inventory.css` (or to your own copy):

```css
.inv-icon.item-rope {
    background-image: url( "s2r://panorama/images/custom_game/uikit/items/rope.vsvg" );
}
```

Then `new InventorySlot("rope", 3)`.

## Other calls

- `kit.Inventory.IsShowing(player, layout)` tells whether a bar is on that player's screen. With `layout = null`, on any inventory layout.
- `kit.Inventory.Hide(player, layout)` takes it off. With `layout = null` it hides on every inventory layout.
- `kit.Inventory.HideAll(layout)` does the same for everyone.
- `handle.Clear()` takes the bar off.

After a round restart the bar is painted again as it was.

See [Common concepts](README.md#common-concepts) for `Style`, `Classes` and `Texts` (written as `inv_<key>`).
