# Progress bar

A bar with an optional label and value. Per player, one bar per placement.

| Placement | Where | For |
|---|---|---|
| `Top` | Under the top edge | A capture, a countdown |
| `Crosshair` | Under the crosshair | An action the player is doing: use, defuse, channel |
| `Bottom` | Above the HUD, bottom centre | A reload, a charge |

Bundled layout: `uikit_progress`. Demo command: `css_uikit_demo_progress`.

## Two ways to drive a bar

**Timed.** Give it `Seconds`. One call, and the client runs the bar by itself: no more writes, even for 30 seconds. It hides itself when done. Use it when you know how long something takes.

```csharp
kit.Progress(player, Bars, "Defusing", seconds: 5, style: UiStyle.Warn);
```

**Pushed.** Give it a `Value` from 0 to 1 and call again whenever it changes. The bar eases to the new value. It stays until hidden. Use it when the progress depends on the game.

```csharp
kit.ProgressBars.Show(player, new ProgressOptions
{
    Layout = Bars,
    Placement = ProgressPlacement.Top,
    Label = "Zone captured",
    Value = captured / 100f,
    ShowPercent = true,
});
```

Calling it every tick is fine: a value that did not move to a new step sends nothing.

## Declare the layout

```csharp
static readonly ProgressLayout Bars = new() { Name = "uikit_progress" };
```

| Property | Default | Meaning |
|---|---|---|
| `Placements` | all 3 | Bars the layout has. Any other placement falls back to the first one. |
| `Steps` | `50` | Resolution of a pushed value: `1 / Steps`, so 2% by default. Must match the `step-*` classes of the stylesheet. |
| `Durations` | `1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30, 45, 60` | Seconds a timed bar knows. A duration snaps to the nearest. Must match the `dur-*` classes. |

## Options

```csharp
IUiHandle handle = kit.ProgressBars.Show(player, new ProgressOptions
{
    Layout = Bars,
    Placement = ProgressPlacement.Crosshair,
    Label = "Defusing",
    Seconds = 5,
    Direction = ProgressDirection.Fill,
    Style = UiStyle.Warn,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Label` | none | Text above the bar, left. Collapsed when blank. |
| `ValueText` | none | Text above the bar, right. Collapsed when blank. |
| `ShowPercent` | `false` | Writes the value as a percentage in the value slot. Pushed bars only. `ValueText` wins. |
| `Value` | `0` | Pushed bars: 0 to 1. |
| `Seconds` | `0` | Timed bars: how long it runs. `0` or less makes it a pushed bar. |
| `Direction` | `Fill` | Timed bars: `Fill` goes from empty to full, `Drain` from full to empty. |
| `Placement` | `Crosshair` | Where the bar is. |

A timed bar that is shown again starts over. The text above a timed bar is whatever you wrote: the client does not count for you. To show a counting number, use a pushed bar.

## Other calls

- `kit.ProgressBars.ShowAll(options)` shows the same bar to every connected player.
- `kit.ProgressBars.Hide(player, placement, layout)` hides one bar. With `placement = null` it hides every bar.
- `kit.ProgressBars.HideAll(placement, layout)` does the same for everyone.
- `handle.Clear()` hides that bar.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
