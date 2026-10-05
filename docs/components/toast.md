# Toast

Stacked notifications that dismiss themselves. Per player. Eight possible placements (corners, sides, top and bottom centre), with a draining progress bar.

Bundled layout: `uikit_toast`. Demo command: `css_uikit_demo_toast`.

## Declare the layout

```csharp
static readonly ToastLayout Toasts = new() { Name = "uikit_toast" };
```

| Property | Default | Meaning |
|---|---|---|
| `Placements` | all 8 | Placements the layout has a stack for. Any other placement falls back to the first one. |
| `Depth` | `4` | Cards per placement. When they are all taken, the oldest one is pushed out. |
| `Durations` | `2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30` | Durations the progress bar knows. A toast's duration snaps to the nearest one. |

## Show a toast

```csharp
kit.Toast(player, Toasts, "Saved", "Your loadout was saved.", UiStyle.Success);
```

The full form:

```csharp
IUiHandle handle = kit.Toasts.Show(player, new ToastOptions
{
    Layout = Toasts,
    Title = "Bomb planted",
    Kicker = "Objective",
    Description = "40 seconds left.",
    Style = UiStyle.Danger,
    Seconds = 6,
    Placement = ToastPlacement.TopCenter,
    Enter = ToastAnimation.FromTop,
    Progress = true,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Title` | required | Headline. |
| `Description` | none | Second line. Collapsed when blank. |
| `Kicker` | none | Small accent line above the title. Collapsed when blank. |
| `Seconds` | `4` | Time on screen. `0` or less keeps it until it is cleared. The player cannot dismiss it. |
| `Placement` | `TopRight` | Where it appears. |
| `Enter` / `Exit` | from the placement's side | `FromLeft`, `FromRight`, `FromTop`, `FromBottom`, `Fade`. |
| `Progress` | `true` | Draining bar under the card, while the toast has a duration. |

## Other calls

- `kit.Toasts.ShowAll(options)` shows the same toast to every connected player.
- `kit.Toasts.Clear(player, layout)` removes a player's toasts. With `layout = null`, it clears every toast layout.
- `handle.Clear()` dismisses one toast early, with its exit animation.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
