# Panel

A modal panel with pages in a left menu. Clicking a page switches the content. Each page has optional stat chips and a list of typed rows: header, text, paragraph, info, action button, toggle, stepper. It takes the mouse while open.

Bundled layout: `uikit_panel`. Demo command: `css_uikit_demo_panel`.

## Declare the layout

```csharp
static readonly PanelLayout Panel = new() { Name = "uikit_panel" };
```

| Property | Default | Meaning |
|---|---|---|
| `Pages` | `8` | Entries in the left menu. |
| `Stats` | `4` | Stat chips above the rows of a page. |
| `Rows` | `24` | Rows of the selected page. The content scrolls. |

## Open a panel

Open it with a build function. The panel is rebuilt on open, after every click and once a second, so a callback only changes your state, never the UI, and live content (a player list) stays current.

```csharp
kit.Panels.Open(player, viewer => new PanelOptions
{
    Layout = Panel,
    Title = "Settings",
    Subtitle = viewer.PlayerName,
    Hint = "Changes are saved instantly.",
    Pages =
    [
        new PanelPage("General",
        [
            PanelRow.Header("Display"),
            PanelRow.Toggle("Notifications", "Show toasts.", settings.Notify, _ => settings.Notify = !settings.Notify),
            PanelRow.Stepper("Volume", "0-100", $"{settings.Volume}%", _ => settings.Volume -= 10, _ => settings.Volume += 10),
            PanelRow.Action("Reset", "Restore the defaults.", "Reset", _ => settings.Reset(), UiStyle.Danger),
        ])
        {
            Stats = [new PanelStat("Level", "12", UiStyle.Gold)],
        },
        new PanelPage("About", [PanelRow.Info("Version", "Plugin build", "1.0.0")]),
    ],
});
```

| Row helper | What it shows |
|---|---|
| `PanelRow.Header(title)` | Section title, no control. |
| `PanelRow.Text(title, description)` | A paragraph inside the list, on the row background. |
| `PanelRow.Paragraph(text)` | Free text between rows, without row background. |
| `PanelRow.Info(title, description, value)` | A read-only value on the right. |
| `PanelRow.Action(title, description, button, onClick)` | A button. |
| `PanelRow.Toggle(title, description, on, onToggle)` | An on/off switch. |
| `PanelRow.Stepper(title, description, value, onPrevious, onNext)` | `‹ value ›` arrows. |

A row can also be `Disabled` (clicks dropped), `Selected` (highlighted) or `Nested` (indented under the row above, for an accordion), with a `with` expression: `PanelRow.Info(...) with { Disabled = true }`.

| `PanelOptions` | Default | Meaning |
|---|---|---|
| `Title` | required | Header title. |
| `Subtitle` | empty | Header subtitle. |
| `Hint` | empty | Footer line. Blank hides the footer. |
| `Pages` | required | Pages of the left menu. |
| `OnClose` | none | Called once when the panel leaves the screen. Not called when another `Open` replaces it. |

`kit.Panels.Open(player, options)` with a plain `PanelOptions` opens a fixed panel: page switches still work, nothing is rebuilt.

## Other calls

- `kit.Panels.Refresh(player, layout)` rebuilds now instead of waiting for the next second.
- `kit.Panels.ShowPage(player, page, layout)` selects a page.
- `kit.Panels.Close(player, layout)` closes it.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
