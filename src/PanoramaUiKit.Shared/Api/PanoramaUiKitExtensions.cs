using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Cards;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Dials;
using PanoramaUiKit.Shared.Progress;
using PanoramaUiKit.Shared.Roulette;
using PanoramaUiKit.Shared.Screamers;
using PanoramaUiKit.Shared.Targets;
using PanoramaUiKit.Shared.Toasts;

namespace PanoramaUiKit.Shared.Api;

/// <summary>
/// One-liners for the common case. The full options records are there for everything else.
///
/// <code>
/// static readonly ToastLayout Toasts = new() { Name = "uikit_toast" };
///
/// kit.Toast(player, Toasts, "Toast title", "Toast description", UiStyle.Danger);
/// kit.Banner(Banners, "Banner text", UiStyle.Warn, seconds: 5);
/// kit.Announce(player, Announces, "Announce title", "Announce description", UiStyle.Danger, kicker: "Kicker");
/// kit.Roulette(player, Roulettes, "Title", items, result => Give(result.Player, result.Item));
/// kit.Card(player, Cards, CardPlacement.TopLeft, "Title", "Description", new CardLine("Label", "Value"));
/// kit.Target(player, Targets, "Press E to open", TargetPosition.Below);
/// kit.Progress(player, Bars, "Defusing", seconds: 5);
/// kit.Dial(player, Dials, "Dash", seconds: 8);
/// kit.Screamer(player, Screamers, ScreamerNames.Momo);
/// </code>
/// </summary>
public static class PanoramaUiKitExtensions
{
    public static IUiHandle Toast(this IPanoramaUiKit kit, CCSPlayerController player, ToastLayout layout, string title,
                                  string? description = null, string style = UiStyle.Neutral, float seconds = 4f)
    {
        return kit.Toasts.Show(player, new ToastOptions
        {
            Layout = layout,
            Title = title,
            Description = description,
            Style = style,
            Seconds = seconds,
        });
    }

    /// <summary>Banner for everyone.</summary>
    public static void Banner(this IPanoramaUiKit kit, BannerLayout layout, string text, string style = UiStyle.Info,
                              float seconds = 0f)
    {
        kit.Banners.ShowAll(new BannerOptions
        {
            Layout = layout,
            Text = text,
            Style = style,
            Seconds = seconds,
        });
    }

    public static IUiHandle Banner(this IPanoramaUiKit kit, CCSPlayerController player, BannerLayout layout, string text,
                                   string style = UiStyle.Info, float seconds = 0f)
    {
        return kit.Banners.Show(player, new BannerOptions
        {
            Layout = layout,
            Text = text,
            Style = style,
            Seconds = seconds,
        });
    }

    public static IUiHandle Announce(this IPanoramaUiKit kit, CCSPlayerController player, AnnounceLayout layout, string title,
                                     string? description = null, string style = UiStyle.Neutral, float seconds = 5f,
                                     string? kicker = null)
    {
        return kit.Announcements.Show(player, new AnnounceOptions
        {
            Layout = layout,
            Title = title,
            Description = description,
            Kicker = kicker,
            Style = style,
            Seconds = seconds,
        });
    }

    public static IUiHandle Roulette(this IPanoramaUiKit kit, CCSPlayerController player, RouletteLayout layout, string title,
                                     IReadOnlyList<RouletteItem> items, Action<RouletteResult>? onResult = null,
                                     float seconds = 4f)
    {
        return kit.Roulettes.Spin(player, new RouletteOptions
        {
            Layout = layout,
            Title = title,
            Items = items,
            Seconds = seconds,
            OnResult = onResult,
        });
    }

    /// <summary>Card pinned to a corner. Title, description and lines are all optional.</summary>
    public static IUiHandle Card(this IPanoramaUiKit kit, CCSPlayerController player, CardLayout layout,
                                 CardPlacement placement, string? title = null, string? description = null,
                                 params CardLine[] lines)
    {
        return kit.Cards.Show(player, new CardOptions
        {
            Layout = layout,
            Placement = placement,
            Title = title,
            Description = description,
            Lines = lines,
        });
    }

    /// <summary>Text next to the crosshair.</summary>
    public static IUiHandle Target(this IPanoramaUiKit kit, CCSPlayerController player, TargetLayout layout, string text,
                                   TargetPosition position = TargetPosition.Below, TargetSize size = TargetSize.Medium,
                                   string style = UiStyle.Neutral, float seconds = 0f)
    {
        return kit.Targets.Show(player, new TargetOptions
        {
            Layout = layout,
            Text = text,
            Size = size,
            Position = position,
            Style = style,
            Seconds = seconds,
        });
    }

    /// <summary>
    /// A bar that runs by itself for <paramref name="seconds"/> (a defuse, a cast). For a bar you move
    /// yourself, use <c>kit.ProgressBars.Show</c> with a <c>Value</c>.
    /// </summary>
    public static IUiHandle Progress(this IPanoramaUiKit kit, CCSPlayerController player, ProgressLayout layout,
                                     string? label, float seconds,
                                     ProgressPlacement placement = ProgressPlacement.Crosshair,
                                     ProgressDirection direction = ProgressDirection.Fill, string style = UiStyle.Info)
    {
        return kit.ProgressBars.Show(player, new ProgressOptions
        {
            Layout = layout,
            Label = label,
            Seconds = seconds,
            Placement = placement,
            Direction = direction,
            Style = style,
        });
    }

    /// <summary>
    /// A ring that the client sweeps by itself for <paramref name="seconds"/>: a cooldown (drain, the default)
    /// or a charge (fill). For a ring you move yourself, use <c>kit.Dials.Show</c> with a <c>Value</c>.
    /// </summary>
    public static IUiHandle Dial(this IPanoramaUiKit kit, CCSPlayerController player, DialLayout layout, string? label,
                                 float seconds, DialPlacement placement = DialPlacement.Crosshair,
                                 ProgressDirection direction = ProgressDirection.Drain, DialSize size = DialSize.Medium,
                                 string style = UiStyle.Info)
    {
        return kit.Dials.Show(player, new DialOptions
        {
            Layout = layout,
            Label = label,
            Seconds = seconds,
            Placement = placement,
            Direction = direction,
            Size = size,
            Style = style,
        });
    }

    /// <summary>Full-screen picture and its sound (<see cref="ScreamerNames"/>), for <paramref name="seconds"/>.</summary>
    public static IUiHandle Screamer(this IPanoramaUiKit kit, CCSPlayerController player, ScreamerLayout layout, string name,
                                     float seconds = 0.6f)
    {
        return kit.Screamer(player, layout, name, seconds, playSound: true);
    }

    /// <summary>
    /// Same, choosing the sound. A separate overload so plugins built against the one above keep binding to it.
    /// </summary>
    public static IUiHandle Screamer(this IPanoramaUiKit kit, CCSPlayerController player, ScreamerLayout layout, string name,
                                     float seconds, bool playSound, string? sound = ScreamerNames.Sound)
    {
        return kit.Screamers.Show(player, new ScreamerOptions
        {
            Layout = layout,
            Name = name,
            Seconds = seconds,
            PlaySound = playSound,
            Sound = sound,
        });
    }
}
