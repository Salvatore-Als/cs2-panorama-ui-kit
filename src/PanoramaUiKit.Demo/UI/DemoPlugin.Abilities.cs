using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Abilities;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int AbilityDemoSeconds = 12;

    /// <summary>
    /// The ability bar for 12 seconds, one slot per state: ready, cooldown, hold, blocked. The full
    /// state is sent every second; the kit only sends what changed.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_abilities", "Ability bar with a cooldown and a hold, for 12 seconds.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoAbilities(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        int tick = 0;
        RunEverySecond($"abilities:{target.Slot}", () =>
        {
            if (!target.IsValid)
            {
                return false;
            }

            if (tick >= AbilityDemoSeconds)
            {
                kit.Abilities.Hide(target, DemoLayouts.Abilities);
                return false;
            }

            kit.Abilities.Update(target, new AbilityBarOptions
            {
                Layout = DemoLayouts.Abilities,
                Style = UiStyle.Danger,
                Slots =
                [
                    new AbilitySlot { Key = "M2", Name = "Ready" },
                    new AbilitySlot { Key = "R", Name = "Cooldown", CooldownRemaining = Math.Max(0, 8 - tick) },
                    new AbilitySlot { Key = "F", Name = "Hold", HoldSeconds = 2f, Holding = tick is >= 3 and < 6 },
                    new AbilitySlot { Key = "E", Name = "Blocked", Blocked = true },
                ],
            });
            tick++;
            return true;
        });
    }
}
