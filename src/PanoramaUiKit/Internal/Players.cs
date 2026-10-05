using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace PanoramaUiKit.Internal;

internal static class Players
{
    /// <summary>Connected humans: no bots, no SourceTV.</summary>
    public static IEnumerable<CCSPlayerController> Humans()
    {
        return Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false });
    }
}
