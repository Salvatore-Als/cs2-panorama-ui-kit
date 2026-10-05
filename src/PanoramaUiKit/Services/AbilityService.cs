using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Abilities;

namespace PanoramaUiKit.Services;

internal sealed class AbilityService(LayoutRegistry registry) : ComponentService<AbilityChannel, AbilityLayout>(registry), IAbilityService
{
    public void Update(CCSPlayerController player, AbilityBarOptions options) =>
        WhenWarm(Channel(options.Layout), channel => channel.Update(player, options));

    public void Hide(CCSPlayerController player, AbilityLayout? layout = null)
    {
        foreach (AbilityChannel channel in Targets(layout))
        {
            channel.ClearAll(player);
        }
    }
}
