# Ability bar

A per-player bar of ability tiles: a key hint, a name, a cooldown gauge and a hold gauge. Built to be updated every tick: the kit remembers what it last drew for each tile and only sends what changed, so a steady bar costs nothing.

Bundled layout: `uikit_abilities`. Demo command: `css_uikit_demo_abilities`.

## Declare the layout

```csharp
static readonly AbilityLayout Abilities = new() { Name = "uikit_abilities" };
```

| Property | Default | Meaning |
|---|---|---|
| `Slots` | `4` | Tiles in the layout. |
| `MaxCooldownSeconds` | `120` | Longest cooldown the gauge can draw. Longer ones are clamped. |
| `HoldStepTenths` / `MaxHoldTenths` | `5` / `100` | Steps of the hold gauge, in tenths of a second. |

These numbers must match the ones the layout was built with.

## Update the bar

Call it every tick, with the real state of each ability:

```csharp
kit.Abilities.Update(player, new AbilityBarOptions
{
    Layout = Abilities,
    Style = UiStyle.Purple,
    Slots =
    [
        new AbilitySlot { Key = "F", Name = "Dash", CooldownRemaining = dashCooldown },
        new AbilitySlot { Key = "R", Name = "Shield", HoldSeconds = 1.5f, Holding = isHoldingR },
        new AbilitySlot { Key = "M2", Name = "Scan", Blocked = !hasTarget },
    ],
});
```

| `AbilitySlot` field | Meaning |
|---|---|
| `Key` | Key hint shown on the tile (`"F"`, `"M2"`). Required. |
| `Name` | Ability name under the tile. Required. |
| `CooldownRemaining` | Whole seconds before it can be used again. `0` means ready. Pass the real remaining value every tick: the gauge runs on the client. |
| `HoldSeconds` | Hold duration. `0` for an instant ability. |
| `Holding` | True while the player holds the key. The hold gauge runs over `HoldSeconds`. |
| `Blocked` | Unusable for a reason other than cooldown (no target, too early). |

Slots beyond the layout's `Slots` are ignored. An empty list hides the bar.

## Other calls

- `kit.Abilities.Hide(player, layout)` hides the bar. With `layout = null`, it hides every ability layout.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
