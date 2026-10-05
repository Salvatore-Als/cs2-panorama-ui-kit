# Banner

One sticky line across the top of the screen. Showing a new banner on top of a visible one only rewrites its text and style (the entry animation does not replay), so it is safe to call every second for a countdown.

Bundled layout: `uikit_banner`. Demo command: `css_uikit_demo_banner [text]`.

## Declare the layout

```csharp
static readonly BannerLayout Banners = new() { Name = "uikit_banner" };
```

## Show a banner

```csharp
kit.Banner(Banners, "Warmup ends in 30s", UiStyle.Warn, seconds: 5);           // everyone
kit.Banner(player, Banners, "You are spectating", UiStyle.Info);                // one player
```

The full form:

```csharp
kit.Banners.Show(player, new BannerOptions
{
    Layout = Banners,
    Text = "Overtime",
    Kicker = "Round 31",
    Style = UiStyle.Gold,
    Seconds = 0,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Text` | required | The line. A blank text hides the banner. |
| `Kicker` | none | Small label beside or above the text. Collapsed when blank. |
| `Seconds` | `0` | Time on screen. `0` or less keeps it until it is hidden. |

## Other calls

- `kit.Banners.ShowAll(options)` shows the same banner to every player.
- `kit.Banners.Hide(player, layout)` and `kit.Banners.HideAll(layout)` play the exit animation. With `layout = null`, they hide every banner layout.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
