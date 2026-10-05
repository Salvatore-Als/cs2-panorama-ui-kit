namespace PanoramaUiKit.Shared.Common;

/// <summary>
/// What every component accepts on top of its own fields: a way for a custom layout to carry more
/// than the bundled one without changing this API.
/// </summary>
public abstract record UiContent
{
    /// <summary>
    /// Style name, applied as <c>style-&lt;name&gt;</c> on the component's root panel. See <see cref="UiStyle"/>.
    /// Lowercase letters, digits, '-' and '_' only; anything else is stripped.
    /// </summary>
    public string Style { get; init; } = UiStyle.Neutral;

    /// <summary>
    /// Extra text slots. Key <c>footer</c> is written to <c>{s:&lt;prefix&gt;_footer}</c> - the same
    /// prefix as the component's own slots (<c>t_tr_0_footer</c>, <c>b_footer</c>, <c>a_footer</c>).
    /// A layout without that slot ignores it. Blank values also add <c>empty</c> on a panel with
    /// that id, so a layout can collapse the line.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Texts { get; init; }

    /// <summary>
    /// Extra classes on the component's root panel, beside <c>style-*</c>. For a custom layout's own
    /// variants (<c>"compact"</c>, <c>"pulse"</c>). Same character rules as <see cref="Style"/>.
    /// </summary>
    public IReadOnlyList<string>? Classes { get; init; }
}
