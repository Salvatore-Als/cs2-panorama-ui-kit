# Minimap

A map in a corner of the screen: the map picture, the current zone (everything outside it tinted red), the next one as a white ring, the player's own arrow (followed by the kit) and markers. Optional zoom, centred on the player and following them.

Two layers:

- **global**, set on the layout, the same on every map that shows it: the zones and global markers (points of interest);
- **per player**: the map itself (which picture, where, how big, how zoomed), the player's arrow and markers only they see (a teammate). A player's mark wins over a global one in the same slot.

Positions snap to a 768 x 768 grid (1 px cells on the 768 px big map, a third of a pixel on the 256 px corner one: the same grid on both, so one layout object fits either) and glide 0.1 s, linear, to each new cell: a running player changes cell at every read, so the motion is continuous. Nothing is sent while a marker stays in its cell.

Bundled layout: `uikit_minimap`. Demo command: `css_uikit_demo_minimap [tl|tr|bl|br] [sm|md|lg] [zoom <n>] [map <name>] [shrink|track|off]`; `map de_mirage` previews another map's picture from any server map.

## The big map

`uikit_bigmap` is the same component on a second layout: a 768 px map in the centre of the screen over a dark veil, bigger markers, the player's halo. Same ids, same classes, same calls; its stylesheet is generated from the corner one (`python tools/minimap_css.py` after editing `uikit_minimap.css`, which also writes the corner one's sizes). Open it with `Show`, close it with `Hide`: on a key, at the start of a round, in the lobby.

```csharp
static readonly MinimapLayout BigMap = new() { Name = "uikit_bigmap" };

kit.Minimap.Show(player, new MinimapOptions { Layout = BigMap });   // where am I
kit.Minimap.Hide(player, BigMap);
```

Global calls with a null layout reach both maps: `SetZone(null, ...)`, `SetGlobalMark(null, ...)`, `TrackPlayer(null, ...)`. Personal marks and `TrackPlayerFor` take one layout: call them for each.

The big map uses the same picture and the same `Bounds` as the corner one: pass the same `Map` and `Bounds` to both. A custom picture is drawn at 768 px there, so see [Your own map picture](#your-own-map-picture) for the size to ship.

## Declare the layout

```csharp
static readonly MinimapLayout Minimap = new() { Name = "uikit_minimap" };
```

| Property | Default | Meaning |
|---|---|---|
| `Marks` | `64` | Marker pool, `mm_mark_0`..`mm_mark_63`. Slot 0 is the player's arrow (with a pulsing halo) while `FollowSelf` is on. |
| `Grid` | `768` | `x-*` / `y-*` / `px-*` / `py-*` ladder of the stylesheet. Both bundled stylesheets have 768. |
| `Radii` | `768` | `r-*` ladder: `r-768` is a radius as wide as the map, a pixel a step on the big map. |
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
| `SelfScale` | `1` | Size of the player's own arrow and halo, snapped to 10 % steps from `0.5` to `2.5` (class `self-NNN` on `mm_frame`). Other markers keep their size. |
| `SelfArrow` | classic | Style of the player's own arrow, see `MinimapArrows` (19: `classic`, `chevron`, `shadow`, `needle`, `triangle`, `cone`, `notch`, `diamond`, `wave`, `disc`, `badge`, `neon`, `outline`, `drop`, `reticle`, `trail`, `shield`, `bevel`, `minimal`). Class `arrow-<name>` on `mm_frame`. |
| `FollowInterval` | `0.05` | Seconds between two reads (the markers glide 0.1 s to each new cell, so it looks continuous). |

## Zones

```csharp
kit.Minimap.SetZone(null, MinimapZoneId.Current, new MinimapZone(x, y, radius));             // red
kit.Minimap.SetZone(null, MinimapZoneId.Next, new MinimapZone(nx, ny, nextRadius));           // white
kit.Minimap.SetZone(null, MinimapZoneId.Current, new MinimapZone(nx, ny, nextRadius), 60f);   // shrink over 60 s
kit.Minimap.SetZone(null, MinimapZoneId.Next, null);                                          // take it off
```

World units for the centre and the radius. The current zone is drawn as a hole in a red veil: inside the circle the map is clear, everything else is tinted red. The next one is a white ring.

With a shrink time, the circle moves and shrinks from where it is to the new one. The next zone's shrink is run by the client (one write per player, the time snapped to the nearest of `Durations`). The current zone's is stepped by the kit every 0.1 s, each step gliding there, linear: its veil is sized, not scaled, so the client cannot run it alone. A player who opens the map mid-shrink sees it from where it is by then.

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

`State`: `downed` (red, pulsing), `dead` (faded), `alerted` (pulsing). `Style` replaces the kind's colour: a `UiStyle` name, or one of `MinimapColors` (`white`, `red`, `orange`, `yellow`, `green`, `cyan`, `blue`, `pink`). The icons are the compass's shapes plus `uikit/minimap/arrow.svg`.

## Maps

| Name | Picture |
|---|---|
| `MinimapMaps.Dust2` | shipped with the kit addon: `panorama/images/custom_game/uikit/minimap/maps/de_dust2.png` (1024 px, so the zoom stays sharp) |
| `MinimapMaps.Mirage`, `Inferno`, `Nuke`, `Overpass`, `Vertigo`, `Ancient`, `Anubis`, `Train`, `Office`, `Italy` | the game's own radar, `panorama/images/overheadmaps/<map>_radar_psd`: every client already has it |
| `MinimapMaps.Slot(1)` .. `Slot(8)` | `panorama/images/custom_game/uikit/minimap/maps/slot_<n>.png` + `slot_<n>_png.vtex`, shipped by another addon, with your own `Bounds` |

Bounds of an official map come from its overview file, `resource/overviews/<map>.txt` in the game (`pos_x`, `pos_y`, `scale`): `MinimapBounds.FromOverview(posX, posY, scale)`. The values in `MinimapMaps.Official` are from those files; check them after a map update.

Not yet verified in game: the radar path from a custom layout. If it draws nothing, ship the radar PNGs in a slot instead.

### Your own map picture

The one hard rule: **the picture covers exactly the world rectangle you give as `Bounds`**, edge to edge. Everything on the map (the arrow, markers, zones) is placed by that rectangle; a picture that does not match it puts them in the wrong place.

The picture:

| | |
|---|---|
| Shape | **Square.** It is stretched over a square (256 px in the corner, 768 px for the big map): any other shape is distorted. |
| Size | **1024 x 1024** (like Dust II). **2048 x 2048** if the big map is zoomed a lot: at x4 it shows 3072 px worth. Always a power of 2 (512, 1024, 2048). |
| Format | PNG, RGBA. Transparent parts show the map's dark background. |
| Orientation | North up: the game's +Y towards the top of the picture, +X to the right, as on CS2's radars. |

The bounds, two ways:

```csharp
// The map has a CS2 radar: resource/overviews/<map>.txt gives pos_x, pos_y, scale.
// size = side of that radar picture in pixels (usually 1024), whatever the size of the PNG you ship.
MinimapBounds bounds = MinimapBounds.FromOverview(posX, posY, scale, size: 1024f);

// No radar: measure two opposite corners of the area in game (getpos), and draw the picture to cover exactly that.
MinimapBounds bounds = new(minX, minY, maxX, maxY);
```

`pos_x` / `pos_y` is the world point at the radar's top-left corner, `scale` the world units per pixel: the rectangle is `pos_x .. pos_x + scale * size` across and `pos_y - scale * size .. pos_y` down.

Ship it in a slot, in your addon, without touching the kit:

```
panorama/images/custom_game/uikit/minimap/maps/slot_1.png
panorama/images/custom_game/uikit/minimap/maps/slot_1_png.vtex
```

The `.vtex` is a copy of the kit's `de_dust2_png.vtex` with `m_fileName` pointing at your PNG. Workshop Tools compiles both to `slot_1_png.vtex_c`. There are 32 slots (`MinimapMaps.Slot(1)` .. `Slot(32)`): one per map of your mode.

Then register the map under its name, once, from your plugin:

```csharp
public override void OnAllPluginsLoaded(bool hotReload)
{
    kit.Minimap.RegisterMap("dz_arcadias", new MinimapMap(MinimapMaps.Slot(1), MinimapBounds.FromOverview(posX, posY, scale)));
    kit.Minimap.RegisterMap("br_canyon", new MinimapMap(MinimapMaps.Slot(2), new MinimapBounds(minX, minY, maxX, maxY)));
}
```

From then on the kit draws that map like an official one, on the corner map and the big one alike: `Show` without `Map` takes the server's map and finds its picture and bounds; your arrow, the points, the zones and the followed players all work on it with nothing else to write. No XML, no stylesheet: your addon ships the PNG, your plugin one line per map.

```csharp
kit.Minimap.Show(player, new MinimapOptions { Layout = Minimap });   // on dz_arcadias: slot 1, its bounds
```

`kit.Minimap.FindMap(name)` tells how a map is drawn (registered, else official, else null: nothing is placed). Without registering, `Map = MinimapMaps.Slot(1)` with `Bounds = ...` on each `Show` does the same. To check the match in game, stand at two far apart corners and compare where your arrow is with where you are on the picture; the BR demo's Debug menu prints the cell the kit computes ("Ma position").

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

### A list of players

To follow several players at once, give the list: one marker each, in list order, a colour each.

```csharp
// Every map, for everyone: the players still in the match.
kit.Minimap.TrackPlayers(null, alivePlayers);

// One viewer's maps: their squad, slots 1 to 3, cyan and green.
MinimapTrackList squad = new() { FirstSlot = 1, Slots = 3, Colors = [MinimapColors.Cyan, MinimapColors.Green] };
kit.Minimap.TrackPlayersFor(player, Minimap, squadMates, squad);
kit.Minimap.TrackPlayersFor(player, BigMap, squadMates, squad);
```

| `MinimapTrackList` | Default | Meaning |
|---|---|---|
| `FirstSlot` | `1` | First marker slot of the range. |
| `Slots` | `32` | Slots in the range. Players past it are not drawn. |
| `Kind` | `mate` | Kind of every marker (an arrow turns with the player's view). |
| `Colors` | `MinimapColors.All` | The n-th player takes the n-th colour, round and round. Null = the kind's colour. |
| `Names` | `true` | The player's name under their marker. |
| `ShowDead` | `true` | Dead: the marker stays where they fell, faded. |

Call it again whenever the list changes: players no longer in it are taken off, the slots it leaves free are cleared. An empty list clears the range. Each player is followed as with `TrackPlayer` (death, disconnect, reconnect).

## Other calls

| Call | Does |
|---|---|
| `ShowAll(options)` | `Show` for every player. |
| `IsShowing(player, layout?)` | True while a map is on their screen. |
| `Hide(player, layout?)` / `HideAll(layout?)` | Takes the map off; the global layer stays. The player's own marks and `TrackPlayerFor` tracks on that layout go with it: set them again after the next `Show`. |
| `Reset(layout?)` | Hides every map and drops the zones and global marks: a new match. |

Survives a round restart: the layout is rebuilt and everything repainted.

## Calibrate a map live: the dev menu

`css_uikit_dev` (admin, `AdminPermission` in the kit's config) opens the kit's dev menu on its **Map** page, for the map the server is on. Everything applies at once, on every open map, yours and every plugin's: no restart, no upload.

| Page | What it does |
|---|---|
| Map | Your world position, where you are on the picture (percent from the top-left, and the cell sent), the bounds, and where they come from: `officielle`, `plugin` (RegisterMap), `dev` (this menu, saved or not). |
| Image | Which picture: the map's official radar, or slot 1 to 32. |
| Coins | Top-left corner here, bottom-right corner here, make it square, centre it on you. |
| Ajuster | Move the bounds left / right / up / down, grow, shrink, by a step of 10, 50, 250 or 1000 units. |
| Afficher et code | Corner map and big map on / off, drop numbered pink landmarks where you stand, print the `RegisterMap` line (and the equivalent `pos_x` / `pos_y` / `scale`) to your console, save, forget. |

The way to calibrate a new map: open the corner map, stand on something you can recognise on the picture (a corner of a building, the edge of the map), drop a landmark, and nudge the bounds until the landmark sits on it. The menu takes the mouse: close it to walk, `css_uikit_dev` again to come back.

"Sauvegarder" writes every map calibrated here to `maps.json` next to the kit plugin, loaded at every start. A calibration from this menu wins over a plugin's `RegisterMap` for the same map, so what you set in game stays. To ship it in your plugin instead, "Imprimer le code" and paste the line; then "Oublier la calibration dev" and save.
