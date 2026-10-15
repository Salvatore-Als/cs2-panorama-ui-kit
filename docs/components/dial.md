# Dial

A ring that sweeps like a clock hand, with a value in the middle and a label under it. For cooldowns, charges and channels. Per player, one dial per placement.

| Placement | Where |
|---|---|
| `Crosshair` | Centred on the crosshair |
| `Bottom` | Above the HUD, bottom centre |
| `BottomLeft` / `BottomRight` | Above the HUD, in the corners |

Four sizes: `Small`, `Medium` (default), `Large`, `ExtraLarge`.

Bundled layout: `uikit_dial`. Demo command: `css_uikit_demo_dial`.

> **Not verified in game yet.** The ring is drawn with `clip: radial()`, which is in Valve's property docs but had never been run in this project. On first load, check that the ring starts at 12 o'clock and sweeps clockwise, and that it eases between values. If it starts elsewhere, change the `0deg` start angle in `uikit_dial.css`.

## Two ways to drive a dial

**Timed.** Give it `Seconds`. One call, and the client sweeps the ring by itself: no more writes. It hides itself when done. `Direction` is `Drain` by default (a cooldown), or `Fill` (a charge).

```csharp
kit.Dial(player, Dials, "Dash", seconds: 8, style: UiStyle.Warn);
kit.Dial(player, Dials, "Charge", seconds: 6, DialPlacement.BottomLeft, ProgressDirection.Fill);
```

**Pushed.** Give it a `Value` from 0 to 1 and call again whenever it changes. The ring eases to it. It stays until hidden.

```csharp
kit.Dials.Show(player, new DialOptions
{
    Layout = Dials,
    Placement = DialPlacement.BottomRight,
    Label = "Shield",
    Value = shield / 100f,
    ShowPercent = true,
});
```

Calling it every tick is fine: a value that did not move to a new step sends nothing.

## Declare the layout

```csharp
static readonly DialLayout Dials = new() { Name = "uikit_dial" };
```

| Property | Default | Meaning |
|---|---|---|
| `Placements` | all 4 | Dials the layout has. Any other placement falls back to the first one. |
| `Steps` | `36` | Resolution of a pushed value: `360 / Steps` degrees, so 10°. Must match the `step-*` classes. |
| `Durations` | `1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30, 45, 60` | Seconds a timed dial knows. A duration snaps to the nearest. Must match the `dur-*` classes. |

## Options

| Option | Default | Meaning |
|---|---|---|
| `Label` | none | Text under the ring. Collapsed when blank. |
| `ValueText` | none | Text in the middle of the ring. Collapsed when blank. |
| `ShowPercent` | `false` | Writes the value as a percentage in the middle. Pushed dials only. `ValueText` wins. |
| `Value` | `0` | Pushed dials: 0 to 1. |
| `Seconds` | `0` | Timed dials: how long it runs. `0` or less makes it a pushed dial. |
| `Direction` | `Drain` | Timed dials: `Drain` goes from full to empty, `Fill` from empty to full. |
| `Size` | `Medium` | Ring size. |
| `Placement` | `Crosshair` | Where the dial is. |

A timed dial that is shown again starts over. The text in the middle is whatever you wrote: the client does not count for you. For a number that counts down, use a pushed dial.

## Other calls

- `kit.Dials.ShowAll(options)` shows the same dial to every connected player.
- `kit.Dials.Hide(player, placement, layout)` hides one dial. With `placement = null` it hides every dial.
- `kit.Dials.HideAll(placement, layout)` does the same for everyone.
- `handle.Clear()` hides that dial.

The bottom placements sit where the [progress bar](progress.md) `Bottom` placement and the [toasts](toast.md) are. Move them with the margins in `uikit_dial.css` if you use them together.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
