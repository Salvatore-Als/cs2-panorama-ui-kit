# Minimap

A map in a corner of the screen: the map picture, the current zone in red, the next one in white, the player's own arrow (followed by the kit) and markers. Optional zoom, centred on the player and following them.

Two layers:

- **global**, set on the layout, the same on every map that shows it: the zones and global markers (points of interest);
- **per player**: the map itself (which picture, where, how big, how zoomed), the player's arrow and markers only they see (a teammate). A player's mark wins over a global one in the same slot.

Positions snap to a 64 x 64 grid (4 px cells on the 256 px map) and glide between cells; nothing is sent while a marker stays in its cell.

Bundled layout: `uikit_minimap`. Demo command: `css_uikit_demo_minimap [tl|tr|bl|br] [sm|md|lg] [zoom <n>] [map <name>] [shrink|track|off]`; `map de_mirage` previews another map's picture from any server map.

## The big map

`uikit_bigmap` is the same component on a second layout: a 768 px map in the centre of the screen over a dark veil, bigger markers, the player's halo. Same ids, same classes, same calls; its stylesheet is generated from the corner one (`python tools/bigmap_css.py` after editing `uikit_minimap.css`). Open it with `Show`, close it with `Hide`: on a key, at the start of a round, in the lobby.

```csharp
static readonly MinimapLayout BigMap = new() { Name = "uikit_bigmap" };

kit.Minimap.Show(player, new MinimapOptions { Layout = BigMap });   // where am I
kit.Minimap.Hide(player, BigMap);
```

Global calls with a null layout reach both maps: `SetZone(null, ...)`, `SetGlobalMark(null, ...)`, `TrackPlayer(null, ...)`. Personal marks and `TrackPlayerFor` take one layout: call them for each.

## Declare the layout

```csharp
static readonly MinimapLayout Minimap = new() { Name = "uikit_minimap" };
```

| Property | Default | Meaning |
|---|---|---|
| `Marks` | `16` | Marker pool, `mm_mark_0`..`mm_mark_15`. Slot 0 is the player's arrow (with a pulsing halo) while `FollowSelf` is on. |
| `Grid` | `64` | `x-*` / `y-*` / `px-*` / `py-*` ladder of the stylesheet. |
| `Radii` | `64` | `r-*` ladder: `r-64` is a radius as wide as the map. |
| `Zooms` | `150, 200, 300, 400` | `zoom-*` classes, in percent. |
| `Durations` | 5 s .. 10 min | `dur-*` classes a zone shrink can run over. |

## Show the map

```csharp
kit.Minimap.Show(player, new MinimapOptions
{
    Layout = Minimap,
    Map = MinimapMaps.Mirage,          // null = the server's current map
    Placement = MinimapPlacement.TopRight,
    Size = MinimapSize.Medium,
    Zoom = 2f,                          // 1 = the whole map
    Title = "Mirage",
});
```

| Option | Default | Meaning |
|---|---|---|
| `Map` | current map | `map-<name>` on `mm_map`: the picture. See [Maps](#maps). |
| `Bounds` | official bounds of `Map` | The world rectangle of the picture. Required for a map that is not in `MinimapMaps.Official`; without bounds nothing is placed. |
| `Placement` | `TopRight` | `at-tr`, `at-tl`, `at-bl`, `at-br`. Top-left covers the game's radar; top-right the kill feed. |
| `Size` | `Medium` | `size-sm` / `size-md` / `size-lg`: 80 / 100 / 125 % of 256 px (a `ui-scale`, not yet verified in game). |
| `Zoom` | `1` | Above 1, snapped to `Zooms` and centred on the player's arrow, following it. Marker icons keep their size; zone rings scale with the map. |
| `Title` | none | `{s:mm_title}` under the map. Blank collapses it. |
| `FollowSelf` | `true` | The kit reads the player's position and view every `FollowInterval` and puts their arrow in slot 0. Dead: the last position, faded. |
| `FollowInterval` | `0.05` | Seconds between two reads (the markers glide 0.06 s between cells, so it looks continuous). |

## Zones

```csharp
kit.Minimap.SetZone(null, MinimapZoneId.Current, new MinimapZone(x, y, radius));             // red
kit.Minimap.SetZone(null, MinimapZoneId.Next, new MinimapZone(nx, ny, nextRadius));           // white
kit.Minimap.SetZone(null, MinimapZoneId.Current, new MinimapZone(nx, ny, nextRadius), 60f);   // shrink over 60 s
kit.Minimap.SetZone(null, MinimapZoneId.Next, null);                                          // take it off
```

World units for the centre and the radius. With a shrink time, the circle moves and shrinks from where it is to the new one, run by the client: one write per player, nothing after. The time snaps to the nearest of `Durations`. A player who opens the map mid-shrink sees it from where it is by then.

## Markers

```csharp
kit.Minimap.SetGlobalMark(Minimap, new MinimapMark(2, MinimapNames.NpcBoss, x, y, Label: "Boss"));   // everyone
kit.Minimap.SetMark(player, Minimap, new MinimapMark(1, MinimapNames.Mate, mx, my, Yaw: mateYaw,
                                                     State: MinimapNames.Downed, Label: "Duo"));   // this player
kit.Minimap.ClearGlobalMark(Minimap, 2);
kit.Minimap.ClearMark(player, Minimap, 1);
```

| Kind | Icon | Colour | Order |
|---|---|---|---|
| `self` | arrow, turned by `Yaw` | yellow | top |
| `mate` | arrow | cyan | |
| `ping` | flag | orange | |
| `npc-boss` | skull, larger | red | |
| `cluster` | dot, put the count in `Label` | red | |
| `npc` | skull | red | |
| `loot` | crate | gold | |
| `dot` | dot | white | bottom |

`State`: `downed` (red, pulsing), `dead` (faded), `alerted` (pulsing). `Style` replaces the kind's colour. The icons are the compass's shapes plus `uikit/minimap/arrow.svg`.

## Maps

| Name | Picture |
|---|---|
| `MinimapMaps.Dust2` | shipped with the kit addon: `panorama/images/custom_game/uikit/minimap/maps/de_dust2.png` (1024 px, so the zoom stays sharp) |
| `MinimapMaps.Mirage`, `Inferno`, `Nuke`, `Overpass`, `Vertigo`, `Ancient`, `Anubis`, `Train`, `Office`, `Italy` | the game's own radar, `panorama/images/overheadmaps/<map>_radar_psd`: every client already has it |
| `MinimapMaps.Slot(1)` .. `Slot(8)` | `panorama/images/custom_game/uikit/minimap/maps/slot_<n>.png` + `slot_<n>_png.vtex`, shipped by another addon, with your own `Bounds` |

Bounds of an official map come from its overview file, `resource/overviews/<map>.txt` in the game (`pos_x`, `pos_y`, `scale`): `MinimapBounds.FromOverview(posX, posY, scale)`. The values in `MinimapMaps.Official` are from those files; check them after a map update.

Not yet verified in game: the radar path from a custom layout. If it draws nothing, ship the radar PNGs in a slot instead.

## Follow a player

```csharp
// On every map: the kit moves the marker, turns it with the player's view, writes their name.
kit.Minimap.TrackPlayer(Minimap, target, new MinimapTrack { Slot = 2, Kind = MinimapNames.NpcBoss, Label = "Wanted" });

// On one player's map only: their duo.
kit.Minimap.TrackPlayerFor(player, Minimap, mate, new MinimapTrack { Slot = 1 });   // Kind = Mate, Label = name

kit.Minimap.ClearGlobalMark(Minimap, 2);      // stops a global track
kit.Minimap.ClearMark(player, Minimap, 1);    // stops a personal one
```

| Event | Marker |
|---|---|
| Moves, turns | follows, 10 times a second (only what changed is sent) |
| Dies | stays where they fell, `state-dead` (faded). `ShowDead = false` hides it until they respawn |
| Disconnects | hidden |
| Reconnects | back, found by SteamID (a bot is followed by its slot, so not across a reconnect) |

Slot 0 is the viewer's own arrow; track in 1 and up. A `SetMark` / `SetGlobalMark` on a tracked slot replaces the track.

## Other calls

| Call | Does |
|---|---|
| `ShowAll(options)` | `Show` for every player. |
| `IsShowing(player, layout?)` | True while a map is on their screen. |
| `Hide(player, layout?)` / `HideAll(layout?)` | Takes the map off; the global layer stays. The player's own marks and `TrackPlayerFor` tracks on that layout go with it: set them again after the next `Show`. |
| `Reset(layout?)` | Hides every map and drops the zones and global marks: a new match. |

Survives a round restart: the layout is rebuilt and everything repainted.
