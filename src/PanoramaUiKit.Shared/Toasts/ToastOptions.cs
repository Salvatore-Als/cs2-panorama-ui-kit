using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Toasts;

/// <summary>Everything a toast can be. Only <see cref="Title"/> is required.</summary>
public sealed record ToastOptions : UiContent
{
    /// <summary>Headline. Slot <c>{s:t_&lt;place&gt;_&lt;n&gt;_title}</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Second line. Slot <c>{s:t_&lt;place&gt;_&lt;n&gt;_desc}</c>. Blank collapses it (<c>empty</c>).</summary>
    public string? Description { get; init; }

    /// <summary>Small accent line above the title. Slot <c>{s:t_&lt;place&gt;_&lt;n&gt;_kicker}</c>. Blank collapses it.</summary>
    public string? Kicker { get; init; }

    /// <summary>Seconds on screen. Zero or less stays until cleared - the player cannot dismiss it.</summary>
    public float Seconds { get; init; } = 4f;

    /// <summary>Falls back to the layout's first placement when the layout does not have this one.</summary>
    public ToastPlacement Placement { get; init; } = ToastPlacement.TopRight;

    /// <summary>Null picks the side the placement sits on.</summary>
    public ToastAnimation? Enter { get; init; }

    /// <summary>Null leaves the way it came in.</summary>
    public ToastAnimation? Exit { get; init; }

    /// <summary>Draining bar under the card, while the toast has a duration.</summary>
    public bool Progress { get; init; } = true;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required ToastLayout Layout { get; init; }
}
