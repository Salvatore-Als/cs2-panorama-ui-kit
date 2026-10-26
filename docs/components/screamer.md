# Screamer

A full-screen picture with its sound, for one player, for a fraction of a second. For a jump scare.

The picture is one class of the stylesheet, `scare-<name>`, whose `background-image` fills the screen. The sound is played by the kit on the player's client (`play <file>`) in the same tick. It is not a CSS `sound`: a stylesheet sound is loaded on use and starts after the picture.

The kit precaches the bundled sound and pictures on every map (`PrecacheScreamers` in the kit config, on by default). Pictures and sounds you add for your own names are not in that list: precache them from your plugin (`OnServerPrecacheResources`).

Bundled layout: `uikit_screamer`. Demo command: `css_uikit_demo_screamer [name] [seconds]`.

## Declare the layout

```csharp
static readonly ScreamerLayout Screamers = new() { Name = "uikit_screamer" };
```

## Show a screamer

```csharp
kit.Screamer(player, Screamers, ScreamerNames.Momo, seconds: 0.6f);
```

The full form:

```csharp
IUiHandle handle = kit.Screamers.Show(player, new ScreamerOptions
{
    Layout = Screamers,
    Name = ScreamerNames.Momo,
    Seconds = 0.6f,
    PlaySound = true,
    Sound = ScreamerNames.Sound,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Name` | required | Which screamer, applied as `scare-<name>`. A blank name hides it. |
| `Seconds` | `0.6` | Time on screen. `0` or less keeps it until it is hidden. |
| `SoundLeadSeconds` | `1` | The sound starts this long before the picture, since the client is slow to start a sound. `0` = both at once. |
| `PlaySound` | `true` | Play `Sound` when the screamer shows. `false` = picture only. The server plays it on the client (`play`), not the stylesheet: a CSS `sound` is loaded on use and arrives late. |
| `Sound` | `sounds/jumpscare.vsnd` | `.vsnd` file played on the player's client when the screamer shows. Blank = silent. |

A screamer shown while another is up swaps the picture in place. The same one again only extends it, without its sound. Check `IsShowing` first to keep one per player.

## The bundled screamers

`ScreamerNames.All`: `momo`, `babadook`, `jeff`, `shadow`.

Every screamer plays the same sound by default, `sounds/jumpscare.vsnd` (from `workshop/sounds/jumpscare.wav`), shipped with the kit addon. Pass `Sound` for another file.

The kit addon ships the rules, the sound and the pictures (`workshop/`). Another addon can provide pictures for its own names:

| File in the addon | Compiles to |
|---|---|
| `panorama/images/custom_game/uikit/screamer/<name>.png` and `<name>_png.vtex` | `<name>_png.vtex_c` |

Picture: 1920x1080, stretched to the screen. A missing picture draws nothing; the screamer still opens, plays the sound and closes.

The tool in `tools/screamer` writes the pictures: pick one, choose the screamer, Create. It fits the picture on black at 1920x1080 and writes its `.vtex`. The names come from `uikit_screamer.css`.

```bash
cd tools
pip install -e ".[screamer]"                  # Pillow
python3 -m screamer                            # writes into workshop/
python3 -m screamer --addon ../../my-addon     # or into another addon's content folder
```

To add another, add a rule to `uikit_screamer.css` (or to your own copy) and use its name:

```css
.screamer.scare-banshee {
    background-image: url( "s2r://panorama/images/custom_game/uikit/screamer/banshee_png.vtex" );
}
```

## Other calls

- `kit.Screamers.IsShowing(player, layout)` tells whether a screamer is on that player's screen. With `layout = null`, on any screamer layout.
- `kit.Screamers.Hide(player, layout)` takes it off. With `layout = null` it hides on every screamer layout.
- `kit.Screamers.HideAll(layout)` does the same for everyone.
- `handle.Clear()` takes that screamer off.

After a round restart a timed screamer is not brought back.

See [Common concepts](README.md#common-concepts) for `Classes`.
