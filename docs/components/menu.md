# Menu

A menu card with paginated entries and sub-menus. A sub-menu opens on top of its parent, with a back button that returns to the parent on the page it was left on. It takes the mouse while open.

Bundled layout: `uikit_menu`. Demo command: `css_uikit_demo_menu`.

## Declare the layout

```csharp
static readonly MenuLayout Menu = new() { Name = "uikit_menu" };
```

| Property | Default | Meaning |
|---|---|---|
| `Items` | `8` | Entries per page. A longer menu gets a pager. |
| `Choices` | `6` | Choices a select entry's dropdown can list. Extra choices are ignored. |

## Open a menu

```csharp
kit.Menus.Open(player, new MenuOptions
{
    Layout = Menu,
    Title = "Settings",
    Subtitle = "Server options",
    Entries =
    [
        MenuEntry.Open("Weapons", _ => BuildWeaponsMenu()),
        MenuEntry.Toggle("Notifications", on: true, (p, on) => SetNotify(p, on)),
        MenuEntry.Select("Difficulty", ["Easy", "Normal", "Hard"], selected: 1, (p, index, choice) => SetDifficulty(index)),
        MenuEntry.Button("Respawn", p => p.Respawn()),
    ],
});
```

| Entry factory | On click |
|---|---|
| `MenuEntry.Button(label, onClick)` | Runs `onClick`. |
| `MenuEntry.Toggle(label, on, onToggle)` | Flips the switch and reports the new state. |
| `MenuEntry.Select(label, choices, selected, onChoose)` | Steps to the next choice and reports its index and text. |
| `MenuEntry.Open(label, buildSubmenu)` | Opens the sub-menu on top. It is built on click, so it can be live. |

Every factory takes an optional `description` (a second line). An entry can also carry a `Value` (right-hand text such as a price), a `Style` and `Disabled`, with a `with` expression: `MenuEntry.Button(...) with { Value = "$2700", Disabled = true }`.

Toggle and select state is kept by the kit, per player and per open menu, so a menu declared once works without storing anything. The callbacks report each change.

| `MenuOptions` | Default | Meaning |
|---|---|---|
| `Title` | required | Card title. |
| `Subtitle` | none | Line under the title. Collapsed when blank. |
| `Entries` | required | The entries, paginated by the layout's `Items`. |
| `OnClose` | none | Called once when the whole menu closes. Only the root menu's callback runs: going back from a sub-menu is not a close. |

## Other calls

- `kit.Menus.Push(player, menu)` opens a menu on top of the current one, with a back button.
- `kit.Menus.Back(player)` returns to the parent menu. On the root menu, it closes.
- `kit.Menus.Close(player, layout)` closes it.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
