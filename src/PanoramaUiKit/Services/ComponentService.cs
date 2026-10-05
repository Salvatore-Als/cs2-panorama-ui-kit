using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Core;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Services;

/// <summary>
/// What every service does besides its own calls: find the channel of the layout a call names, the
/// channels a layout-less call targets, and fan a call out to every human.
/// </summary>
internal abstract class ComponentService<TChannel, TLayout>
    where TChannel : LayoutChannel
    where TLayout : UiLayout
{
    private readonly LayoutRegistry _registry;

    protected ComponentService(LayoutRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>The channel for <paramref name="layout"/>; null when the registry refused it (logged).</summary>
    protected TChannel? Channel(TLayout layout) => _registry.Get<TChannel>(layout);

    /// <summary>
    /// Runs <paramref name="action"/> on the channel once its layout entity exists. The first Open of
    /// a layout spawns it (~200 ms), which inside a client's command gets the client dropped: until
    /// the warm-up timers are done, the call waits.
    /// </summary>
    protected void WhenWarm(TChannel? channel, Action<TChannel> action)
    {
        if (channel == null)
        {
            return;
        }

        _registry.WhenWarm(channel, () => action(channel));
    }

    /// <summary>
    /// <see cref="WhenWarm(TChannel?, Action{TChannel})"/> for a call that returns a handle. While it
    /// waits the caller holds a provisional handle that follows the real one once it exists.
    /// </summary>
    protected IUiHandle WhenWarm(TChannel? channel, CCSPlayerController player, Func<TChannel, IUiHandle> show)
    {
        if (channel == null)
        {
            return Dead;
        }

        if (channel.IsWarm)
        {
            return show(channel);
        }

        UiHandle pending = new(player);
        _registry.WhenWarm(channel, () => pending.Link(show(channel)));
        return pending;
    }

    /// <summary>That layout's channel, or every live <typeparamref name="TLayout"/> channel when null.</summary>
    protected IReadOnlyList<TChannel> Targets(TLayout? layout) => _registry.Resolve<TChannel, TLayout>(layout);

    protected static void ForEachHuman(Action<CCSPlayerController> action)
    {
        foreach (CCSPlayerController player in Players.Humans())
        {
            action(player);
        }
    }

    protected static IUiHandle Dead => UiHandle.Dead;
}
