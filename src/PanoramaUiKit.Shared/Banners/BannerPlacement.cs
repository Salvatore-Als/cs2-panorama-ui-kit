using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Banners;

/// <summary>Where the banner sits: <c>at-*</c> on <c>b_card</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<BannerPlacement>))]
public enum BannerPlacement
{
    /// <summary>Top centre, under the game's team bar. The default.</summary>
    UnderHud,

    /// <summary>Against the top edge, over the team bar.</summary>
    Top,

    /// <summary>
    /// Under a match bar with <see cref="Match.MatchOptions.Band"/> and a zone status at
    /// <see cref="Zone.ZonePlacement.Band"/>, clear of both.
    /// </summary>
    UnderBand,

    /// <summary>Bottom centre, above the inventory row.</summary>
    Bottom,
}

public static class BannerPlacementExtensions
{
    /// <summary>Class on the panel: <c>at-hud</c>, <c>at-top</c>, <c>at-band</c>, <c>at-bottom</c>.</summary>
    public static string Class(this BannerPlacement placement) => placement switch
    {
        BannerPlacement.Top => "at-top",
        BannerPlacement.UnderBand => "at-band",
        BannerPlacement.Bottom => "at-bottom",
        _ => "at-hud",
    };
}
