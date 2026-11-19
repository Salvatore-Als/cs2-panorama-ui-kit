# Match bar

A server-wide status bar at the top: a block on each side (a big count and its label) around a centre (a timer and a line under it), with an optional score tab under each side. Every player sees the same thing.

```
  [  6  ]   [  1:45  ]   [  6  ]
  [  T  ]   [  Live  ]   [ CT  ]
    [ 3 ]               [ 5 ]        <- score tabs
```

Bundled layout: `uikit_match`. Demo command: `css_uikit_demo_match`.

## Declare the layout

```csharp
static readonly MatchLayout Match = new() { Name = "uikit_match" };
```

## Update the bar

Safe to call every tick: text is only sent when it changes, and players who join late (or after a round restart) get the bar on the next call.

```csharp
kit.Match.Update(new MatchOptions
{
    Layout = Match,
    LeftLabel = "T",  LeftValue = $"{aliveT}",  LeftStyle = UiStyle.Warn,  LeftScore = scoreT,
    RightLabel = "CT", RightValue = $"{aliveCt}", RightStyle = UiStyle.Info, RightScore = scoreCt,
    Center = MatchOptions.FormatTime(secondsLeft),
    CenterSub = "Live",
    Urgent = secondsLeft <= 10,
});
```

| Option | Default | Meaning |
|---|---|---|
| `LeftLabel` / `RightLabel` | dash | Label under each count (`"T"`, `"Alive"`, a team name). |
| `LeftValue` / `RightValue` | dash | The big count on each side. |
| `LeftStyle` / `RightStyle` | `neutral` | Colour of each side block and its score tab. |
| `LeftScore` / `RightScore` | none | Score in the tab under each side. |
| `ShowScore` | `true` | Shows the score tabs. They are also hidden when neither score is given. |
| `Center` | dash | Centre text. `MatchOptions.FormatTime(seconds)` formats a `m:ss` clock. |
| `CenterSub` | none | Small line under the centre. Collapsed when blank. |
| `Urgent` | `false` | Last seconds: the timer turns red and pulses. |
| `Band` | `false` | `band` on `m_bar`: a long opaque strip across the top (1400 px, fading at both ends) over the game's team bar, and the bar hangs under it. `Style` colours the strip's hairline. |

### Band

The strip is empty on purpose: it is room for other components, which take the `Band` placement.

```
==========[ compass: CompassPlacement.Band ]==========    strip, 62 px
          [   6   |    1:45    |    6   ]                 the bar, under it
          [ ALIVE |   ZONE 2   |  KILLS ]
              [ zone status: Band ]
              [ banner: BannerPlacement.UnderBand ]
```

```csharp
kit.Match.Update(new MatchOptions { Layout = Match, Band = true, /* ... */ });
kit.Compass.Show(player, new CompassOptions { Layout = Compass, Placement = CompassPlacement.Band });
kit.Zone.Show(player, new ZoneOptions { Layout = Zones, Placement = CompassPlacement.Band, /* ... */ });
```

The match layout draws under the other kit layouts (`z-index` 99990 against 99999), so whatever sits on the strip stays on top of it. The width and the height of the strip live in `uikit_match.css` (`.match-band`); the gap under the bar for the zone status in `uikit_zone.css` (`.zone-status.at-band`). Score tabs, when shown, overlap a zone status at `Band`.

## Other calls

- `kit.Match.Hide(layout)` hides the bar for everyone until the next `Update`. With `layout = null`, it hides every match layout.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
