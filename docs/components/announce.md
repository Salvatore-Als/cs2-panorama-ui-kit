# Announce

A big centred card for reveals, objectives and round intros: a kicker, a title and a description. One per player per layout. Showing a new one while another is visible plays the exit of the first, then the new card enters.

Bundled layout: `uikit_announce`. Demo command: `css_uikit_demo_announce`.

## Declare the layout

```csharp
static readonly AnnounceLayout Announces = new() { Name = "uikit_announce" };
```

## Show an announce

```csharp
kit.Announce(player, Announces, "You are the impostor", "Eliminate everyone.", UiStyle.Danger, kicker: "Your role");
```

The full form:

```csharp
kit.Announcements.Show(player, new AnnounceOptions
{
    Layout = Announces,
    Title = "Round 1",
    Kicker = "Get ready",
    Description = "Defend site A.",
    Style = UiStyle.Info,
    Seconds = 5,
    AnimateTitle = true,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Title` | required | Big line. |
| `Description` | none | Line under the title. Collapsed when blank. |
| `Kicker` | none | Small line above the title. Collapsed when blank. |
| `AnimateTitle` | `true` | Plays the title's entry animation (scale down and flash). |
| `Seconds` | `5` | Time on screen. `0` or less keeps it until it is hidden. |

## Other calls

- `kit.Announcements.ShowAll(options)` shows it to every player.
- `kit.Announcements.Hide(player, layout)` and `kit.Announcements.HideAll(layout)` remove it. With `layout = null`, they hide every announce layout.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
