# Kill feed

A server-wide feed, newest row on top: kills, or any short event. Every player sees the same rows. Players who join late get the feed on the next push.

Bundled layout: `uikit_killfeed`. Demo command: `css_uikit_demo_killfeed`.

## Declare the layout

```csharp
static readonly KillFeedLayout Feed = new() { Name = "uikit_killfeed" };
```

| Property | Default | Meaning |
|---|---|---|
| `Rows` | `5` | Rows in the layout. A push past that pushes the oldest row out. |

## Push a row

```csharp
kit.KillFeed.Push(new KillFeedEntry
{
    Layout = Feed,
    Attacker = attacker.PlayerName,
    Tag = "AK-47",
    Victim = victim.PlayerName,
    Style = UiStyle.Danger,
    Seconds = 7,
});
```

| Option | Default | Meaning |
|---|---|---|
| `Victim` | required | Right-hand name. |
| `Attacker` | none | Left-hand name. Collapsed when blank, for a feed that never names the killer. |
| `Tag` | none | Caption between the two (weapon, `"HS"`, `"Eliminated"`). Collapsed when blank. |
| `Seconds` | `7` | Time before the row leaves. |

## Other calls

- `kit.KillFeed.Clear(layout)` empties the feed for everyone. With `layout = null`, it clears every kill feed layout.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
