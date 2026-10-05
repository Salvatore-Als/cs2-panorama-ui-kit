# Victory screen

An end-of-round screen: a title, a subtitle, small chips (MVP, best player, a stat) and a countdown to the next round. Per player, so each side can read its own result.

Bundled layout: `uikit_victory`. Demo command: `css_uikit_demo_victory`.

## Declare the layout

```csharp
static readonly VictoryLayout Victory = new() { Name = "uikit_victory" };
```

| Property | Default | Meaning |
|---|---|---|
| `Chips` | `4` | Chips in the layout. Extra chips are ignored. |

## Show the screen

```csharp
kit.Victory.ShowAll(new VictoryOptions
{
    Layout = Victory,
    Title = "Terrorists win",
    Subtitle = "Bomb exploded",
    Kicker = "Round 12",
    Style = UiStyle.Warn,
    Chips =
    [
        new VictoryChip("MVP", mvp.PlayerName, UiStyle.Gold),
        new VictoryChip("Kills", "4", UiStyle.Danger),
    ],
    RestartSeconds = 8,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Title` | required | Big line. |
| `Subtitle` / `Kicker` | none | Lines under and above the title. Collapsed when blank. |
| `Chips` | empty | `VictoryChip(Label, Name, Style)`. An empty list hides the chip row. |
| `RestartSeconds` | `0` | Countdown under the card, ticked by the kit. `0` hides it. Typically `mp_round_restart_delay`. |
| `RestartLabel` | `"Next round in"` | Label of the countdown. |
| `RestartFormat` | `"{0}s"` | Format of the countdown. `{0}` is the seconds left. |
| `Seconds` | `0` | Time on screen. `0` or less keeps it until it is hidden or the round restarts. |

## Other calls

- `kit.Victory.Show(player, options)` shows it to one player, for a per-side result.
- `kit.Victory.Hide(player, layout)` and `kit.Victory.HideAll(layout)` remove it. With `layout = null`, they hide every victory layout.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
