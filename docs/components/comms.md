# Comms

An incoming radio transmission on the left of the screen: a face as a glitching hologram, a name and a line of text. For a villain taunting the players, a mission briefing, a voice on the radio.

The face is one class of the stylesheet, `face-<name>` on `cm_face`, whose rule sets the picture. The picture is white and the stylesheet tints it with the card's style. Everything moves on the client: scanlines, a scan band, a flicker and a jitter every few seconds. After the show, the kit writes nothing more.

The card sits on the left edge, its top at about a third of the screen: under the radar, above the health.

Bundled layout: `uikit_comms`. Demo command: `css_uikit_demo_comms [face] [text...]`.

## Declare the layout

```csharp
static readonly CommsLayout Comms = new() { Name = "uikit_comms" };
```

## Show a transmission

```csharp
IUiHandle handle = kit.Comms.Show(player, new CommsOptions
{
    Layout = Comms,
    Face = CommsFaces.Warden,
    Name = "THE WARDEN",
    Text = "Nobody leaves this island. The zone closes in sixty seconds.",
    Style = UiStyle.Danger,
    Glitch = true,
});
```

`kit.Comms.ShowAll(options)` shows it to every player.

| Option | Default | Meaning |
|---|---|---|
| `Text` | required | What is said, `{s:cm_text}`. Wraps on up to three lines. Revealed by a wipe, left to right. |
| `Face` | `unknown` | Which face, applied as `face-<name>` on `cm_face`. Blank shows `unknown`. |
| `Name` | none | Who speaks, `{s:cm_name}`. Blank collapses the line. |
| `Kicker` | `INCOMING TRANSMISSION` | Over the card, `{s:cm_kicker}`, next to a blinking dot and signal bars. Blank collapses it. |
| `Footer` | none | Under the text, `{s:cm_footer}`. Blank collapses it. |
| `Style` | `info` | Hologram tint: `info` cyan, `danger` red, `success` green, `warn` orange, `purple`, `gold`, `neutral` grey. |
| `Glitch` | `false` | Stronger, faster jitter (`glitch-hard`), for a dramatic line. |
| `ConnectSeconds` | `0.6` | Heavy static (`static`) when the card opens, before the face settles. `0` skips it. |
| `Sound` | none | `.vsnd` file played on the player's client (`play`) when the transmission shows. |
| `Seconds` | `6` | Time on screen. `0` or less keeps it until it is hidden. |

A Show while a transmission is up swaps the face and the text in place, behind a short burst of static, and restarts the timer. The text is wiped in again. The kit alternates `burst-a` and `burst-b` for that: one class turned off and on in the same tick never reaches the client.

The card opens with a CRT power-on (a line that opens up, with a flash) and closes with the reverse.

## The bundled faces

`CommsFaces.All`: `warden` (masked, a visor slit), `jackal` (helmet, triangle lenses), `doctor` (gas mask, two filters), `oracle` (hood, glowing eyes), `unknown` (silhouette, a question mark).

They are SVG files shipped with the kit addon, compiled to `.vsvg_c`:

| File in the addon | Compiles to |
|---|---|
| `panorama/images/custom_game/uikit/comms/<name>.svg` | `<name>.vsvg_c` |

The kit precaches them on every map. A missing picture draws nothing: the card still opens with its name and text.

Unverified in game: a `.vsvg` as `background-image` from an addon, `wash-color` on it, and the looping keyframes (`animation-iteration-count: infinite` runs elsewhere in the kit, the others are untried).

## Add a face

Draw it white on transparent: the stylesheet tints it. Cut the details out of the shape (`fill-rule="evenodd"`) rather than drawing them in another colour, so the tint reads. Square, the face in the upper two thirds.

Add a rule to `uikit_comms.css` (or to your own copy) and use its name:

```css
.comms-face.face-baron .comms-face-layer {
    background-image: url( "s2r://panorama/images/custom_game/uikit/comms/baron.vsvg" );
}
```

A PNG works too, through its `.vtex` like the screamer pictures (`baron_png.vtex`, referenced as `baron_png.vtex`). A colour picture is tinted all the same, unless its rule turns the tint off as `villain` does (`wash-color: #ffffff00` on `.comms-face-main`): a colour photo, 320 x 360 (the portrait's 160 x 180 ratio, twice the size), shown as is.

Precache your own pictures from your plugin (`OnServerPrecacheResources`).

## Other calls

- `kit.Comms.Hide(player, layout)` plays the power-off, then closes. With `layout = null`, on every comms layout.
- `kit.Comms.HideAll(layout)` does the same for everyone.
- `handle.Clear()` takes that transmission off.

After a round restart a transmission without a lifetime is shown again; a timed one is dropped.

See [Common concepts](README.md#common-concepts) for `Classes` and `Texts`.
