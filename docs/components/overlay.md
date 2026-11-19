# Overlay

A full-screen effect over the player's view: frozen, burning, knocked down, poisoned, a zombie's vision... or one of your own, such as your mode's logo, through a custom effect. One effect per player at a time; showing another replaces it.

Bundled layout: `uikit_overlay`. Demo command: `css_uikit_demo_overlay <effect|next|prev|off> [seconds]`.

## Declare the layout

```csharp
static readonly OverlayLayout Overlays = new() { Name = "uikit_overlay" };
```

## Show an effect

```csharp
kit.Overlay(player, Overlays, OverlayEffects.Frozen7, seconds: 4);
```

The full form:

```csharp
IUiHandle handle = kit.Overlays.Show(player, new OverlayOptions
{
    Layout = Overlays,
    Effect = OverlayEffects.Knock1,
    Seconds = 0,
    KeepOnDeath = false,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Effect` | required | The effect, class `fx-<name>` on `ov_fx`. See below. Blank hides the overlay. |
| `Seconds` | `0` | Time on screen. `0` or less keeps it until it is hidden. |
| `KeepOnDeath` | `false` | Most states end with the player: by default the effect goes when they die. |

## The effects

`OverlayEffects`, each constant's summary says what it looks like; the overlay lab page shows them moving.

| Family | Names |
|---|---|
| Frozen | `frozen1` .. `frozen20` |
| Burning | `burn1` .. `burn20` |
| Zombie | `zombie1` (decayed), `zombie2` (rage), `zombie3` (yellow fever) |
| Knocked down | `knock1` (black bars, dark world, slow heartbeat) |
| Other states | `poison`, `electrocuted`, `flashbang`, `bleeding`, `dizzy`, `radiation`, `adrenaline`, `stealth`, `shield`, `heal`, `slowed`, `haste`, `stunned`, `smoke`, `nausea`, `thermal`, `nightvision`, `concussion`, `spotted`, `divine` |
| Custom | `custom1` .. `custom8` (`OverlayEffects.Custom(n)`) |
| Checks | `debug1`, `debug2` |

## Your own effect: a logo, a splash

`custom1` to `custom8` draw a picture your addon ships, over the whole screen, with nothing to change in the kit:

```
panorama/images/custom_game/uikit/overlay/custom/custom_1.png        1920 x 1080, transparent around what it shows
panorama/images/custom_game/uikit/overlay/custom/custom_1_png.vtex   a copy of any of the kit's _png.vtex, m_fileName pointing at the PNG
```

```csharp
kit.Overlay(player, Overlays, OverlayEffects.Custom(1));   // your logo, until hidden
```

Draw the picture at the screen's size and put the logo where you want it on it (a corner, the middle): the kit stretches it over the screen. For anything more (an animated effect, several layers), add it to `EFFECTS` in `tools/overlay_fx.py` and run the script.

## When an effect does not show

`debug1` is a plain magenta screen written in `uikit_overlay.css`, `debug2` a plain cyan one in `uikit_overlay_fx.css`:

- both show: the stylesheets load, the problem is in that effect's rules;
- only `debug1`: `uikit_overlay_fx.css` did not load (check the addon compiled it);
- neither: the overlay itself is not shown (`css_uikit_status`, the client console).

## Other calls

- `kit.Overlays.ShowAll(options)` shows the same effect to every connected player.
- `kit.Overlays.IsShowing(player, layout)` tells whether an effect is up (`null` layout = any).
- `kit.Overlays.Hide(player, layout)` takes it off; `kit.Overlays.HideAll(layout)` for everyone.
- `handle.Clear()` takes that effect off.

## How it is built

`ov_fx` holds a pool of 8 full-screen layers and 40 particles; `fx-<name>` styles the part it uses: gradients, inner glows, a colour panel with `-s2-mix-blend-mode` to tint the world, `world-blur`, SVG textures, `@keyframes`. The rules (`uikit_overlay_fx.css`), the textures (`uikit/overlay/fx/`) and `OverlayEffects` are written by `python tools/overlay_fx.py` from one list. Not yet verified in game: the blend modes and `world-blur` over the world from a custom layout.
