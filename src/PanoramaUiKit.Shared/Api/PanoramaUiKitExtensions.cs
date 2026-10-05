using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Roulette;
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
}
