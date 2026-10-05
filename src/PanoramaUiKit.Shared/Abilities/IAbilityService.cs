using CounterStrikeSharp.API.Core;

namespace PanoramaUiKit.Shared.Abilities;

/// <summary>
/// Per-player ability bar. <see cref="Update"/> is meant to be called every tick: the kit keeps
/// what it last painted per slot and only sends what changed, so a steady bar costs nothing.
/// </summary>
public interface IAbilityService
{
    void Update(CCSPlayerController player, AbilityBarOptions options);

    /// <summary>Null layout = on every ability layout.</summary>
    void Hide(CCSPlayerController player, AbilityLayout? layout = null);
}
