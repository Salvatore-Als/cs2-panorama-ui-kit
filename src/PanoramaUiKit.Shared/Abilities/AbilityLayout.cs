using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Abilities;

/// <summary>Contract "abilities": a pool of <see cref="Slots"/> tiles, ids <c>ab_*</c>.</summary>
public sealed class AbilityLayout : UiLayout
{
    public override string Component => "abilities";

    /// <summary>Tiles <c>ab_0</c>..<c>ab_{Slots-1}</c> in the layout.</summary>
    public int Slots { get; init; } = 8;

    /// <summary>Highest <c>cd-N</c> class (seconds, every integer from 1). Longer cooldowns are clamped.</summary>
    public int MaxCooldownSeconds { get; init; } = 120;

    /// <summary><c>hold-N</c> classes are tenths of a second, by this step, up to <see cref="MaxHoldTenths"/>.</summary>
    public int HoldStepTenths { get; init; } = 5;

    public int MaxHoldTenths { get; init; } = 100;
}
