# Recap

An end-of-match screen for one player: their placement, a headline, and at the bottom, discreet, their name and one line of stats (icon, value, label, split by hairlines). For the screen a player sees when they win the match or are eliminated.

Per player, since every player has their own stats. The whole entry is one `show` class: the stylesheet staggers the placement, the headline, the strip and each tile through `transition-delay`.

Bundled layout: `uikit_recap`. Demo command: `css_uikit_demo_recap [win|lose]`.

## Declare the layout

```csharp
static readonly RecapLayout Recap = new() { Name = "uikit_recap" };
```

| Property | Default | Meaning |
|---|---|---|
| `Stats` | `8` | Stat tiles in the layout. Extra stats are ignored. |

## Show the screen

```csharp
kit.Recap.Show(player, new RecapOptions
{
    Layout = Recap,
    Kicker = "Match over",
    Title = "Last one standing",
    Place = 1,
    Total = 64,
    PlayerSub = "Squad Alpha",
    Style = UiStyle.Gold,
    Stats =
    [
        RecapStats.Kills(7, best: true),
        RecapStats.Knockouts(3),
        RecapStats.Points(1250),
        RecapStats.DamageDealt(1873),
        RecapStats.Survival(TimeSpan.FromMinutes(18.7)),
    ],
    RestartSeconds = 15,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Title` | required | The headline. |
| `Kicker` | none | Line above the placement. Collapsed when blank. |
| `Place` | none | Final placement, written as `PlaceFormat` (`"#{0}"`). Null hides the placement row. |
| `Total` | none | Players in the match, written as `TotalFormat` (`"/ {0}"`) next to the placement. Null collapses it. |
| `Player` | the player's name | Big name on the left of the strip. |
| `PlayerSub` | none | Line under the name (a team, a squad). Collapsed when blank. |
| `Stats` | empty | `RecapStat(Icon, Value, Label, Style, Best)`. An empty list hides the tile row. |
| `Players` | empty | `RecapPlayer(Name, Stats, Sub, Style)`: a duo or a squad, one row each, in place of `Player` and `Stats`. See [Player rows](#player-rows). |
| `RestartSeconds` | `0` | Countdown under the strip, ticked by the kit. `0` hides it. |
| `RestartLabel` | `"Next match in"` | Label of the countdown. |
| `RestartFormat` | `"{0}s"` | Format of the countdown. `{0}` is the seconds left. |
| `Seconds` | `0` | Time on screen. `0` or less keeps it until it is hidden or the round restarts. |

`Style` on the options is the outcome colour: the glow behind the placement, the strip's accent, the kicker and the placement number. `UiStyle.Gold` for a win and `UiStyle.Danger` for an elimination read well.

The stats line is as wide as its stats, centred: three or eight, it stays one quiet line under the headline.

## Stat tiles

`RecapStat` is one tile:

| Field | Meaning |
|---|---|
| `Icon` | Icon name, applied as `icon-<name>` on the tile's icon. See below. |
| `Value` | Big text. Shrinks to fit. |
| `Label` | Small caps line under the value. |
| `Style` | Accent of the tile: its top line and the icon's tint. Default `neutral`. |
| `Best` | Gold border and glow, whatever the style. For a personal best. |

`RecapStats` builds the common ones with their icon, an English label and a style: `Kills`, `Knockouts`, `Assists`, `Points`, `Shots`, `Headshots`, `Accuracy` (0 to 1, written as a percentage), `DamageDealt`, `DamageTaken`, `Revives`, `Survival` (a `TimeSpan`, written `m:ss`), `Distance` (metres). For another label or language, build a `RecapStat` yourself:

```csharp
new RecapStat(RecapIcons.Kills, "7", "Éliminations", UiStyle.Danger, Best: true)
```

## Player rows

When a duo or a squad wins together, give `Players` instead of `Player` / `Stats`: one quiet line per player, the name on the left and its stats on the same line, stacked under the headline. The layout has 4 rows of 4 stats (`RecapLayout.Rows`, `RowStats`).

```csharp
kit.Recap.ShowAll(new RecapOptions
{
    Layout = Recap,
    Kicker = "Victory",
    Title = "Last ones standing",
    Place = 1,
    Total = 48,
    Style = UiStyle.Gold,
    Players =
    [
        new RecapPlayer("Player 1", [RecapStats.Kills(7, best: true), RecapStats.Points(1240)], Style: UiStyle.Gold),
        new RecapPlayer("Player 2", [RecapStats.Kills(5), RecapStats.Points(980)]),
    ],
});
```

`UpdateStat` works on the single player's tiles only.

## Update while shown

A `Show` while the screen is up repaints it in place, without playing the entry again. A tile whose value changed pulses: the kit swaps `pulse-a` and `pulse-b` on it (the same class off and on in one tick never reaches the client).

For one value, `UpdateStat` rewrites the tile at that index (0-based) and pulses it:

```csharp
kit.Recap.UpdateStat(player, 0, "8", Recap);
```

Nothing happens if no recap is up for that player or the tile is not shown.

## The bundled icons

`RecapIcons.All`: `kills`, `knockouts`, `points`, `shots`, `damage_dealt`, `damage_taken`, `headshots`, `revives`, `survival`, `accuracy`, `distance`, `assists`.

They are white SVGs shipped with the kit addon, tinted by the tile's style (`wash-color`):

| File in the addon | Compiles to |
|---|---|
| `panorama/images/custom_game/uikit/stats/<name>.svg` | `<name>.vsvg_c` |

Not verified in game yet: a `.vsvg` from the addon used as a `background-image`. Valve's own layouts load `.vsvg` icons, but none of the kit's layouts did before this one. If a tile shows no icon, check the client console for the image path.

To add another, add a rule to `uikit_recap.css` (or to your own copy) and use its name:

```css
.recap-stat-icon.icon-loot {
    background-image: url( "s2r://panorama/images/custom_game/uikit/stats/loot.vsvg" );
}
```

## Other calls

- `kit.Recap.ShowAll(options)` shows it to every human, each with their own name when `Player` is null. Stats are usually per player: prefer `Show` per player.
- `kit.Recap.Hide(player, layout)` and `kit.Recap.HideAll(layout)` play the exit and remove it. With `layout = null`, they hide every recap layout.
- `handle.Clear()` takes that screen off.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
