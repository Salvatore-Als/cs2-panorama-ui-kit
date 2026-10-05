# Roulette

A case-opening roulette: a strip of cards scrolls past a marker and slows down until it stops on the winner. The winner is drawn on the server when the spin starts, from each item's weight. The strip is only the show. Per player.

Bundled layout: `uikit_roulette`. Demo command: `css_uikit_demo_roulette [fast|slow]`.

## Declare the layout

```csharp
static readonly RouletteLayout Roulettes = new() { Name = "uikit_roulette" };
```

| Property | Default | Meaning |
|---|---|---|
| `Cells` | `31` | Cards on the strip. |
| `WinnerCell` | `24` | The card the strip stops on. |
| `Durations` | `1, 2, 3, 4, 5, 6, 8, 10` | Spin durations the stylesheet has a transition for. |

`Cells` and `WinnerCell` must match the layout: the stylesheet scrolls exactly far enough to centre that card.

## Spin

```csharp
RouletteItem[] items =
[
    new("AK-47", "Common", UiStyle.Neutral, Weight: 10, Tag: "weapon_ak47"),
    new("AWP", "Rare", UiStyle.Info, Weight: 3, Tag: "weapon_awp"),
    new("Knife", "Legendary", UiStyle.Gold, Weight: 1, Tag: "weapon_knife"),
];

kit.Roulette(player, Roulettes, "Open the case", items, result =>
{
    if (!result.Interrupted)
    {
        result.Player.GiveNamedItem((string)result.Item.Tag!);
    }
});
```

| `RouletteItem` field | Meaning |
|---|---|
| `Name` | Main line of the card. |
| `Sub` | Second line. Collapsed when no item has one. |
| `Style` | Rarity colour. |
| `Weight` | Relative chance: 1 against 3 is one in four. `0` or less takes the item out of the draw. |
| `Tag` | Yours. Comes back untouched in the result. |

The full form, `kit.Roulettes.Spin(player, new RouletteOptions { ... })`, also takes:

| Option | Default | Meaning |
|---|---|---|
| `Kicker` | none | Small line above the title. |
| `Seconds` | `4` | Spin duration, which sets its speed. Snaps to the nearest of `Durations`. |
| `HoldSeconds` | `3` | Time the result stays once the strip has stopped. `0` or less keeps it until cleared. |
| `ResultLabel` | `"You got"` | Label above the result line. |
| `OnResult` | none | Called once when the strip stops, on the server thread. |

`RouletteResult.Interrupted` is true when the spin ended early: cleared, replaced by another spin, the player left, or the round restarted. The item was still drawn; whether to award it is your call.

## Other calls

- `kit.Roulettes.SpinAll(options)` spins for every player, each with their own draw.
- `kit.Roulettes.Hide(player, layout)` and `kit.Roulettes.HideAll(layout)` end the spin.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
