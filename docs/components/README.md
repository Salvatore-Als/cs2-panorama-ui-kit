# Components

| Component | What it is | Scope |
|---|---|---|
| [Toast](toast.md) | Stacked notifications that dismiss themselves | Per player |
| [Banner](banner.md) | One sticky line across the top | Per player or everyone |
| [Announce](announce.md) | Big centred card: reveal, objective, round intro | Per player or everyone |
| [Ability bar](abilities.md) | Ability tiles with cooldown and hold gauges | Per player |
| [Match bar](match.md) | Top status bar: two sides and a timer | Everyone |
| [Kill feed](killfeed.md) | Feed of short events, newest on top | Everyone |
| [Victory screen](victory.md) | End-of-round screen with chips and a countdown | Per player or everyone |
| [Roulette](roulette.md) | Case-opening strip on a weighted draw | Per player |
| [Vote](vote.md) | Question, choices with live counts, countdown | Per player |
| [Panel](panel.md) | Modal with pages and typed rows | Per player |
| [Menu](menu.md) | Paginated menu with sub-menus | Per player |

Open `previews/uikit_<component>.preview.html` in a browser for a rough look at each bundled layout.

## Common concepts

**Layouts.** Every call names the layout file it draws on, through a layout object declared once:

```csharp
static readonly ToastLayout Toasts = new() { Name = "uikit_toast" };
```

`Name` is the file name without folder or extension: `uikit_toast` is `panorama/layout/custom_game/uikit_toast.xml`. Any other file that follows the same contract can replace it without changing the calls. See [the contracts](../contracts/README.md).

**Preload.** Call `kit.Preload(layout)` for every layout your plugin uses, from `OnAllPluginsLoaded`. Creating a layout on first use sends the client one big burst in the same tick.

**Handles.** `Show`, `Open` and `Spin` return an `IUiHandle`. `handle.IsVisible` tells whether it is still on screen, `handle.Clear()` dismisses it early. It is safe to keep and clear after it is gone.

**Every options record also takes:**

- `Style`: a colour name, applied as the class `style-<name>`. The bundled layouts define `UiStyle.Neutral`, `Info`, `Success`, `Warn`, `Danger`, `Purple` and `Gold`. A custom layout can define its own.
- `Texts`: extra text slots for a custom layout. `Texts["footer"]` is written to the component's `<prefix>_footer` slot.
- `Classes`: extra classes on the component's main panel, for a custom layout's variants (`"compact"`, `"pulse"`).

**Clearing everything.** `kit.ClearAll(player)` removes everything the kit shows to a player, `kit.ClearEveryone()` to everyone.

**Threads.** Native calls are not thread-safe. After an `await`, come back through `Server.NextFrame` before calling the kit.
