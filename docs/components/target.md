# Target

A short, discreet line of text next to the crosshair: above it, below it, to its left or to its right. For a prompt ("Press E to open") or the name of what the player is aiming at.

One line only: no second line, no accent bar, just a faint veil behind the text. The label keeps a fixed gap from the screen centre and grows away from it, so it never covers the crosshair. Per player, one label per position, so the four can be up at once.

Bundled layout: `uikit_target`. Demo command: `css_uikit_demo_target [above|below|left|right]`.

## Declare the layout

```csharp
static readonly TargetLayout Targets = new() { Name = "uikit_target" };
```

| Property | Default | Meaning |
|---|---|---|
| `Positions` | all 4 | Positions the layout has a label for. Any other position falls back to the first one. |

## Show a text

```csharp
kit.Target(player, Targets, "Press E to open", TargetPosition.Below, TargetSize.Medium, UiStyle.Success);
```

The full form:

```csharp
IUiHandle handle = kit.Targets.Show(player, new TargetOptions
{
    Layout = Targets,
    Position = TargetPosition.Below,
    Text = "Press E to open",
    Size = TargetSize.Medium,
    Style = UiStyle.Success,
    Seconds = 0,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Text` | required | The text. A blank text hides that position. |
| `Size` | `Medium` | `Small`, `Medium`, `Large` or `ExtraLarge` (classes `size-sm`, `size-md`, `size-lg`, `size-xl`). |
| `Position` | `Below` | `Above`, `Below`, `Left` or `Right`. |
| `Seconds` | `0` | Time on screen. `0` or less keeps it until it is hidden. |

Show again at the same position to rewrite it in place.

## Other calls

- `kit.Targets.ShowAll(options)` shows the same text to every connected player.
- `kit.Targets.Hide(player, position, layout)` hides one position. With `position = null` it hides every position. With `layout = null` it hides on every target layout.
- `kit.Targets.HideAll(position, layout)` does the same for everyone.
- `handle.Clear()` hides that text.

The layout assumes the crosshair is at the screen centre. To change the gap, edit the margins in `uikit_target.css`.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
