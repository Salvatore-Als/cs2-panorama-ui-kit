using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Shared.Abilities;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.KillFeed;
using PanoramaUiKit.Shared.Match;
using PanoramaUiKit.Shared.Menus;
using PanoramaUiKit.Shared.Panels;
using PanoramaUiKit.Shared.Roulette;
using PanoramaUiKit.Shared.Toasts;
using PanoramaUiKit.Shared.Victory;
using PanoramaUiKit.Shared.Votes;

namespace PanoramaUiKit.Core;

/// <summary>
/// One channel (one layout entity) per layout file, whoever asks for it. Spawned on first use.
///
/// Refuses, once and loudly in the log, what would fail silently in game: one file used as two
/// different components, and a layout whose declared shape cannot work. Root ids are derived from
/// the file name (<c>&lt;name&gt;_root</c>), so one file per channel also means one root id per channel:
/// no two layouts can share text slots.
/// </summary>
internal sealed class LayoutRegistry : IDisposable
{
    private readonly BasePlugin _plugin;
    private readonly ILogger _logger;
    private readonly Dictionary<string, LayoutChannel> _channels = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _reported = new();

    public LayoutRegistry(BasePlugin plugin, ILogger logger)
    {
        _plugin = plugin;
        _logger = logger;
    }

    public IEnumerable<LayoutChannel> Channels => _channels.Values;

    private readonly List<(LayoutChannel Channel, Action Action)> _waiting = new();

    /// <summary>Raised when a layout entity has to be spawned: a channel was created, or a call is waiting on one.</summary>
    public event Action? WarmWanted;

    /// <summary>Runs <paramref name="action"/> now if the layout entity exists, else once it has been warmed up.</summary>
    public void WhenWarm(LayoutChannel channel, Action action)
    {
        if (channel.IsWarm)
        {
            action();
            return;
        }

        _waiting.Add((channel, action));
        WarmWanted?.Invoke();
    }

    /// <summary>Warms the next layout whose entity is not spawned yet. False when none is left.</summary>
    public bool WarmNext(CCSPlayerController player)
    {
        LayoutChannel? cold = _channels.Values.FirstOrDefault(c => !c.IsWarm);
        if (cold == null)
        {
            return false;
        }

        cold.WarmUp(player);
        if (cold.IsWarm)
        {
            Release(cold);
        }

        return _channels.Values.Any(c => !c.IsWarm);
    }

    /// <summary>Runs the calls that waited for this channel, in the order they came.</summary>
    private void Release(LayoutChannel channel)
    {
        List<Action> ready = _waiting.Where(w => w.Channel == channel).Select(w => w.Action).ToList();
        _waiting.RemoveAll(w => w.Channel == channel);
        foreach (Action action in ready)
        {
            action();
        }
    }

    /// <summary>New map: every layout entity is gone, and the calls that waited for one with it.</summary>
    public void CoolAll()
    {
        _waiting.Clear();
        foreach (LayoutChannel channel in _channels.Values)
        {
            channel.Cool();
        }
    }

    /// <summary>
    /// The channels a call targets: the one for <paramref name="layout"/> (spawned if needed), or with
    /// a null layout every live channel drawing a <typeparamref name="TLayout"/>.
    /// </summary>
    public IReadOnlyList<TChannel> Resolve<TChannel, TLayout>(TLayout? layout)
        where TChannel : LayoutChannel
        where TLayout : UiLayout
    {
        if (layout != null)
        {
            TChannel? channel = Get<TChannel>(layout);
            if (channel == null)
            {
                return [];
            }

            return [channel];
        }

        return _channels.Values.OfType<TChannel>().Where(c => c.Layout is TLayout).ToList();
    }

    /// <summary>The channel for this layout, spawned if needed. Null when refused (already logged).</summary>
    public T? Get<T>(UiLayout layout) where T : LayoutChannel
    {
        if (_channels.TryGetValue(layout.Name, out LayoutChannel? existing))
        {
            if (existing is T typed && existing.Layout.GetType() == layout.GetType())
            {
                return typed;
            }

            Refuse(layout, $"already used as a '{existing.Layout.Component}' layout");
            return null;
        }

        string? problem = Check(layout);
        if (problem != null)
        {
            Refuse(layout, problem);
            return null;
        }

        LayoutChannel channel = Create(layout);
        _channels[layout.Name] = channel;
        _logger.LogInformation("Panorama UI Kit: {Component} layout '{Name}' spawned ({Path}, root '{Root}')",
            layout.Component, layout.Name, layout.Path, layout.RootPanelId);
        WarmWanted?.Invoke();
        return (T)channel;
    }

    private LayoutChannel Create(UiLayout layout) => layout switch
    {
        ToastLayout toast => new ToastChannel(_plugin, toast),
        BannerLayout => new CardChannel(_plugin, layout, CardBehaviour.Banner),
        AnnounceLayout => new CardChannel(_plugin, layout, CardBehaviour.Announce),
        AbilityLayout abilities => new AbilityChannel(_plugin, abilities),
        MatchLayout match => new MatchChannel(_plugin, match),
        KillFeedLayout feed => new KillFeedChannel(_plugin, feed),
        VictoryLayout victory => new VictoryChannel(_plugin, victory),
        RouletteLayout roulette => new RouletteChannel(_plugin, roulette),
        VoteLayout vote => new VoteChannel(_plugin, vote),
        PanelLayout panel => new PanelChannel(_plugin, panel),
        MenuLayout menu => new MenuChannel(_plugin, menu),
        _ => throw new ArgumentOutOfRangeException(nameof(layout), layout.GetType().Name, "Unknown layout type"),
    };

    private static string? Check(UiLayout layout)
    {
        if (string.IsNullOrWhiteSpace(layout.Name))
        {
            return "no file name";
        }

        return layout switch
        {
            ToastLayout { Depth: < 1 } => "Depth must be at least 1",
            ToastLayout { Placements.Count: 0 } => "no Placements",
            ToastLayout { Durations.Count: 0 } => "no Durations",
            AbilityLayout { Slots: < 1 } => "Slots must be at least 1",
            AbilityLayout { MaxCooldownSeconds: < 1 } => "MaxCooldownSeconds must be at least 1",
            KillFeedLayout { Rows: < 1 } => "Rows must be at least 1",
            VictoryLayout { Chips: < 0 } => "Chips cannot be negative",
            VoteLayout { Choices: < 2 } => "Choices must be at least 2",
            RouletteLayout { Cells: < 3 } => "Cells must be at least 3",
            RouletteLayout { Durations.Count: 0 } => "no Durations",
            RouletteLayout roulette when roulette.WinnerCell < 0 || roulette.WinnerCell >= roulette.Cells => "WinnerCell must be one of the Cells",
            PanelLayout { Pages: < 1 } => "Pages must be at least 1",
            PanelLayout { Rows: < 1 } => "Rows must be at least 1",
            PanelLayout { Stats: < 0 } => "Stats cannot be negative",
            MenuLayout { Items: < 1 } => "Items must be at least 1",
            MenuLayout { Choices: < 1 } => "Choices must be at least 1",
            _ => null,
        };
    }

    private void Refuse(UiLayout layout, string reason)
    {
        if (_reported.Add($"{layout.Name}|{reason}"))
        {
            _logger.LogError("Panorama UI Kit: {Component} layout '{Name}' refused: {Reason}. Nothing will be drawn on it.",
                layout.Component, layout.Name, reason);
        }
    }

    public void ClearAll(CCSPlayerController player)
    {
        foreach (LayoutChannel channel in _channels.Values)
        {
            channel.ClearAll(player);
        }
    }

    public void ClearEveryone()
    {
        foreach (LayoutChannel channel in _channels.Values)
        {
            if (channel is KillFeedChannel feed)
            {
                feed.Clear();
            }
            else if (channel is MatchChannel match)
            {
                match.Hide();
            }
            else
            {
                channel.ClearEveryone();
            }
        }
    }

    public void Forget(int slot)
    {
        foreach (LayoutChannel channel in _channels.Values)
        {
            channel.Forget(slot);
        }
    }

    public void Dispose()
    {
        foreach (LayoutChannel channel in _channels.Values)
        {
            channel.Dispose();
        }

        _channels.Clear();
    }
}
