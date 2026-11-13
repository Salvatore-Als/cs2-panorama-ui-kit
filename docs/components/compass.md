# Compass

A horizontal compass at the top centre of the screen, under the game's team bar (alive players, round timer), with pins on it. It follows the player's view by itself: one Show, nothing to call after. Pins mark a world point (with its distance) or a fixed heading.

The strip is one class, `hd-N`, which slides it under the caret with a transform. The kit reads the player's view every 0.1 s and writes the class only when it changes, so a player standing still costs nothing. Pins sit at their heading from the caret (`rel-N`); a pin behind the player waits at the edge of the bar with an arrow (`edge-l`, `edge-r`).

Bundled layout: `uikit_compass`. Demo command: `css_uikit_demo_compass [pins]` (toggles it, with up to four pins around the caller).

## Declare the layout

```csharp
static readonly CompassLayout Compass = new() { Name = "uikit_compass" };
```

## Show the compass

```csharp
kit.Compass.Show(player, new CompassOptions
{
    Layout = Compass,
    Pins =
    [
        new CompassPin(safeZoneCentre, CompassNames.Zone, UiStyle.Info),
        new CompassPin(cratePosition, CompassNames.Crate, UiStyle.Gold),
        CompassPin.AtBearing(225f, CompassNames.Flag, UiStyle.Danger),
    ],
});
```

`safeZoneCentre` and `cratePosition` are `System.Numerics.Vector3` in game units.

| Option | Default | Meaning |
|---|---|---|
| `Follow` | `true` | The kit reads the player's view and position and moves the strip and the pins. A dead or spectating player keeps the last heading. `false` = `Heading`. |
| `FollowInterval` | `0.05` | Seconds between two reads while following; also the floor. The strip glides 0.06 s between 2.5° steps. |
| `Heading` | `0` | Heading shown when not following, in degrees, 0 = north, clockwise. Call Show again to move it. |
| `Pins` | none | Only the first `CompassLayout.Pins` (4) are drawn. |
| `Style` | `neutral` | `style-*` on `cp_bar`: colour of the caret and of the north. |
| `Placement` | `UnderHud` | `at-*` on `cp_bar`. `UnderHud`: top centre, under the game's team bar (alive players, round timer). `Top`: against the top edge, for a server that hides that bar. `Bottom`: above the inventory row. `TopLeft`: under the radar and the money. `TopRight`: under the kill feed. `Right`: right edge, vertically centred. `Band`: in the strip of a match bar with `Band` (see the match page). |

A Show on a player who already has the compass replaces its options in place. It stays until hidden, through a round restart.

## Pins

| Field | Default | Meaning |
|---|---|---|
| `Position` | required | World point. The pin sits at its heading from the player. |
| `Kind` | `dot` | Shape, `kind-<name>` (`CompassNames`). |
| `Style` | `neutral` | Colour, `style-*`. |
| `ShowDistance` | `true` | Writes the distance under the pin: `240m`, `1.2km`. |
| `Bearing` | null | Fixed heading instead of a point, no distance. `CompassPin.AtBearing(225f)`. |

Replace the pins without showing it again, for a moving target:

```csharp
kit.Compass.SetPins(player, [new CompassPin(teammate.Position, CompassNames.Dot, UiStyle.Success)]);
```

`SetPins` returns `false` when no compass is up.

## Headings

North is +Y on the map, east is +X: the heading is `90 - yaw`. `CompassNames.HeadingFromYaw(yaw)` and `CompassNames.HeadingTo(fromX, fromY, toX, toY)` do the conversion for your own use. Distances are game units at 0.0254 m each.

## The bundled shapes

`CompassNames.All`: `dot`, `diamond`, `zone`, `skull`, `flag`, `crate`. Each is a white picture tinted by the pin's style:

| File in the addon | Compiles to |
|---|---|
| `panorama/images/custom_game/uikit/compass/<name>.svg` | `<name>.vsvg_c` |

To add another, add a rule to `uikit_compass.css` (or your own copy) and use its name as `Kind`:

```css
.compass-pin.kind-ally .compass-pin-icon {
    background-image: url( "s2r://panorama/images/custom_game/uikit/compass/ally.vsvg" );
}
```

Unverified in game: an addon `.vsvg` as `background-image` (Valve's own layouts use their `.vsvg` that way), and how the strip looks while sliding. After one and a half turns the same way the strip runs out and jumps back by a full turn once.

## Other calls

- `kit.Compass.IsShowing(player, layout)` tells whether the compass is on that player's screen. With `layout = null`, on any compass layout.
- `kit.Compass.Hide(player, layout)` takes it off. With `layout = null` it hides on every compass layout.
- `kit.Compass.HideAll(layout)` does the same for everyone.
- `handle.Clear()` takes it off.

See [Common concepts](README.md#common-concepts) for `Classes` and `Texts`.
