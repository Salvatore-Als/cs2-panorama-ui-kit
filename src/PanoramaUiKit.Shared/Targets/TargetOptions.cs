using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Targets;

public sealed record TargetOptions : UiContent
{
    /// <summary>The text. Slot <c>{s:tg_&lt;pos&gt;_text}</c>. Blank hides the label at that position.</summary>
    public required string Text { get; init; }

    /// <summary>Text size. Applied as <c>size-sm|md|lg|xl</c>.</summary>
    public TargetSize Size { get; init; } = TargetSize.Medium;

    /// <summary>Falls back to the layout's first position when the layout does not have this one.</summary>
    public TargetPosition Position { get; init; } = TargetPosition.Below;

    /// <summary>Seconds on screen. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required TargetLayout Layout { get; init; }
}
