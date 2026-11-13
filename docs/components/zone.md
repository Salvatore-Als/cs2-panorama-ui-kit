# Zone

The safe zone, for one player at a time. Two parts on one layout, each shown on its own:

- the **status**, a pill under the compass: phase, state, a countdown ticked by the kit every second, a thin bar drained by the client, and an optional info line. It stays until hidden.
- the **alert**, a card in the upper third between two hazard stripes, for when the zone moves. It goes after a few seconds.

The layout closes once neither is up.

Bundled layout: `uikit_zone`. Demo command: `css_uikit_demo_zone [seconds|alert|out|off] [hud|top|band|bottom|tl|tr|bl|br|right] [slot]`.

## Declare the layout

```csharp
static readonly ZoneLayout Zones = new() { Name = "uikit_zone" };
```

## Show the status

Send it at the start of each phase. The kit ticks the countdown and the client drains the bar on its own: nothing to send in between.

```csharp
IUiHandle handle = kit.Zone.Show(player, new ZoneOptions
{
    Layout = Zones,
    Phase = "Zone 2",
    State = ZoneState.Closing,
    Seconds = 45,          // left on the countdown
    TotalSeconds = 60,     // length of the phase: the bar starts at 45/60
    Info = "Safe zone 240 m",
    Outside = false,
});

kit.Zone.ShowAll(options);  // the same status for everyone
```

| Option | Default | Meaning |
|---|---|---|
| `Phase` | `null` | `{s:zn_phase}`: "Zone 2". Blank collapses it. |
| `State` | `Waiting` | `state-waiting`, `state-closing` or `state-final` on `zn_status`: the colour and icon. |
| `StateText` | the state's own | `{s:zn_state}`. Defaults: "Closing in", "Closing", "Final zone". |
| `Seconds` | `0` | Seconds left on the countdown, ticked by the kit. It stops at `0:00` and stays: send the next phase. `0` = no countdown. |
| `TotalSeconds` | `0` | Length of the phase, for the bar. `0` hides the bar. |
| `TimerFormat` | `{0}:{1:00}` | `string.Format` pattern, `{0}` = minutes, `{1}` = seconds. |
| `Info` | `null` | `{s:zn_info}`, a second line: a distance, a count of players alive. Blank collapses it. |
| `Outside` | `false` | `outside` on `zn_status`: red, pulsing, the warning icon. |
| `Placement` | `UnderHud` | `at-*` on `zn_status` (`ZonePlacement`). `UnderHud`, `Top`, `Band`, `Bottom`: under the compass at the placement of the same name (above it at the bottom); `Band` is under the match bar hanging from its band. `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`: a corner, clear of the radar, kill feed, health and ammo. `Right`: right edge, centred. |
| `Icon` | `null` | `icon-<name>` on `zn_status`: replaces the state's icon. See [Icons](#icons). Outside the zone the warning glyph still wins. |

A status sent again with the same `TotalSeconds` and an end within 1.5 s of the running one keeps its bar running, so a plugin can push it every second (to update `Info`) without restarting the bar.

The bar is a clip, not a width: `step-N` snaps it to the share of the phase left (20 steps), `dur-N` is the seconds left, `run` drains it. The time left snaps to the nearest of `ZoneLayout.Durations` (every 5 s to 2 min, then every 30 s to 5 min); the countdown text stays exact.

## Outside the safe zone

Flip it without sending the whole status, from your own position check:

```csharp
kit.Zone.SetOutside(player, outside: true, Zones);
```

## Alert

```csharp
kit.Zone.Alert(player, new ZoneAlertOptions
{
    Layout = Zones,
    Kicker = "Warning",
    Title = "The zone is closing",
    Description = "Head for the safe zone.",
    Style = UiStyle.Danger,
    Seconds = 4,
});

kit.Zone.AlertAll(alert);
```

| Option | Default | Meaning |
|---|---|---|
| `Kicker` | `Warning` | `{s:zn_alert_kicker}`. Blank collapses it. |
| `Title` | required | `{s:zn_alert_title}`. |
| `Description` | `null` | `{s:zn_alert_desc}`. Blank collapses it. |
| `Style` | `warn` | Colour of the stripes, kicker and icon. `danger` for the last zone. |
| `Seconds` | `4` | Time on screen, exit included. `0` or less keeps it until it is hidden. |
| `Icon` | `null` | `icon-<name>` on `zn_alert`: replaces the icon. See [Icons](#icons). |

The alert works with or without the status. A new alert while one is up replaces its text.

## Icons

White SVGs shipped with the kit addon, tinted by the stylesheet with `wash-color`:

| File in the addon | Used for |
|---|---|
| `panorama/images/custom_game/uikit/zone/ring.svg` | Status, waiting |
| `panorama/images/custom_game/uikit/zone/closing.svg` | Status closing or final, the alert |
| `panorama/images/custom_game/uikit/zone/danger.svg` | Status, outside |

They are referenced as `.vsvg` (`s2r://panorama/images/custom_game/uikit/zone/ring.vsvg`). Not yet verified in game: an addon SVG used as a `background-image`. A missing icon draws nothing; the rest of the widget is unaffected.

### Choosing the icon

By default the state picks the icon. `ZoneOptions.Icon` / `ZoneAlertOptions.Icon` override it with a class `icon-<name>`; `ZoneIcons` lists the names the bundled stylesheet knows:

| Name | Picture | Tint |
|---|---|---|
| `ZoneIcons.Ring`, `Closing`, `Danger` | the built-ins above | the state's / the style's colour |
| `ZoneIcons.Slot(1)` .. `Slot(8)` | `panorama/images/custom_game/uikit/zone/slots/slot_<n>_png.vtex` | none, drawn as is |

The slots are pictures **this addon does not ship**: another addon provides them, so a game mode gets its own art on the same widget without forking the layout or the stylesheet. That addon ships, per slot, a PNG and its `.vtex` (same shape as the screamer pictures):

```
panorama/images/custom_game/uikit/zone/slots/slot_1.png
panorama/images/custom_game/uikit/zone/slots/slot_1_png.vtex   -> slot_1_png.vtex_c
```

```csharp
kit.Zone.Show(player, new ZoneOptions { Layout = Zones, Icon = ZoneIcons.Slot(1), /* ... */ });
kit.Zone.Alert(player, new ZoneAlertOptions { Layout = Zones, Title = "Storm incoming", Icon = ZoneIcons.Slot(2) });
```

A slot nobody filled draws nothing. To change a built-in icon everywhere instead, ship a file at the built-in's own path in an addon mounted before this one. Not yet verified: how the resource compiler treats a stylesheet pointing at a slot that is missing at compile time.

## Other calls

- `kit.Zone.Hide(player, layout)` takes the status and the alert off. With `layout = null`, on every zone layout.
- `kit.Zone.HideAll(layout)` does the same for everyone.
- `handle.Clear()` on the status handle takes the status off; on an alert handle, plays the alert's exit.

After a round restart the status comes back with the time it had left; the alert does not.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`. `Texts` of the status are written as `zn_<key>`, those of the alert as `zn_alert_<key>`.
