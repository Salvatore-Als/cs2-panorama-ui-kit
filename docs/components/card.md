# Card

An info card pinned to a corner of the screen, or to the bottom centre: a title, a description and label / value lines. Every part is optional, so a card can be just a title, just a description, just lines, or any mix. It has the same look as the [announce](announce.md) card, with no animation: it is there as soon as it is shown.

Four corners and the bottom centre. The top left one sits where the radar is. The bottom ones sit above the HUD. Per player, one card per placement.

Bundled layout: `uikit_card`. Demo command: `css_uikit_demo_card`.

## Declare the layout

```csharp
static readonly CardLayout Cards = new() { Name = "uikit_card" };
```

| Property | Default | Meaning |
|---|---|---|
| `Placements` | all 5 | Placements the layout has a card for. Any other one falls back to the first. |
| `Lines` | `4` | Lines per card. Extra lines are dropped. |

## Show a card

```csharp
kit.Card(player, Cards, CardPlacement.TopLeft, "Objective", "Hold the zone.",
    new CardLine("Time left", "42s"), new CardLine("Captured", "30%"));
```

Any part can be left out:

```csharp
kit.Card(player, Cards, CardPlacement.TopRight, title: "Only a title");
kit.Card(player, Cards, CardPlacement.BottomLeft, description: "Only a description.");
kit.Card(player, Cards, CardPlacement.BottomRight, lines: [new CardLine("Kills", "12"), new CardLine("Deaths", "4")]);
```

The full form:

```csharp
IUiHandle handle = kit.Cards.Show(player, new CardOptions
{
    Layout = Cards,
    Placement = CardPlacement.TopLeft,
    Title = "Objective",
    Description = "Hold the zone until the timer ends.",
    Lines = [new CardLine("Time left", "42s"), new CardLine("Captured", "30%")],
    Style = UiStyle.Warn,
    Seconds = 0,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Title` | none | Headline. Collapsed when blank. |
| `Description` | none | Paragraph that wraps. Collapsed when blank. |
| `Lines` | none | `CardLine(Label, Value)`: a muted label on the left, a value on the right. Either can be left out. A blank line is skipped. |
| `Placement` | `TopLeft` | `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` or `BottomCenter`. |
| `Seconds` | `0` | Time on screen. `0` or less keeps it until it is hidden. |

A card with no title, no description and no line is hidden.

## Update a card

Show again on the same corner: the card is rewritten in place. It is safe to call every second for a live value, and only what changed is sent.

## Other calls

- `kit.Cards.ShowAll(options)` shows the same card to every connected player.
- `kit.Cards.Hide(player, placement, layout)` hides one corner. With `placement = null` it hides every corner. With `layout = null` it hides on every card layout.
- `kit.Cards.HideAll(placement, layout)` does the same for everyone.
- `handle.Clear()` hides that card.

The top right corner is also where the [kill feed](killfeed.md) lives, and the bottom corners are where the [toasts](toast.md) stack. Move the card with the margins in `uikit_card.css` if you use them together.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
