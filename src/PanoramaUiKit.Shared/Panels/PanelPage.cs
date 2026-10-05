namespace PanoramaUiKit.Shared.Panels;

/// <summary>
/// One page: an entry in the left menu (<c>{s:p_page_&lt;n&gt;_label}</c>) and the rows shown when it
/// is selected. Only the selected page's stats and rows are on screen.
/// </summary>
public sealed record PanelPage(string Label, IReadOnlyList<PanelRow> Rows)
{
    /// <summary>Chips above the rows. Only the first layout <c>Stats</c> are shown.</summary>
    public IReadOnlyList<PanelStat> Stats { get; init; } = [];
}
