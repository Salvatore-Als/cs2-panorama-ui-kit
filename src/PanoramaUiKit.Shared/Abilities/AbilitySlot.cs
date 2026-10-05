namespace PanoramaUiKit.Shared.Abilities;

/// <summary>One slot of the ability bar, as the caller sees it this tick.</summary>
public readonly record struct AbilitySlot
{
    /// <summary>Key hint ("F", "R", "M2"). Slot <c>{s:ab_&lt;n&gt;_key}</c>.</summary>
    public required string Key { get; init; }

    /// <summary>Ability name under the tile. Slot <c>{s:ab_&lt;n&gt;_name}</c>.</summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whole seconds before it can be used again; 0 = ready. Slot <c>{s:ab_&lt;n&gt;_cd}</c> and class
    /// <c>cooldown</c>. The gauge is started once, client side, over what is left - pass the real
    /// remaining value each tick, the kit only writes when it changes.
    /// </summary>
    public int CooldownRemaining { get; init; }

    /// <summary>Hold duration of the ability, 0 for an instant one.</summary>
    public float HoldSeconds { get; init; }

    /// <summary>True while the player is holding the key: class <c>holding</c>, hold gauge runs over <see cref="HoldSeconds"/>.</summary>
    public bool Holding { get; init; }

    /// <summary>Unusable for a reason other than a cooldown (too early, no target...): class <c>blocked</c>.</summary>
    public bool Blocked { get; init; }
}
