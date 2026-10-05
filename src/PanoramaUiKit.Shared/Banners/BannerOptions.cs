using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Banners;

public sealed record BannerOptions : UiContent
{
    /// <summary>The line. Slot <c>{s:b_text}</c>. Blank hides the banner.</summary>
    public required string Text { get; init; }

    /// <summary>Small label beside or above the text. Slot <c>{s:b_kicker}</c>. Blank collapses it.</summary>
    public string? Kicker { get; init; }

    /// <summary>Seconds on screen. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required BannerLayout Layout { get; init; }
}
