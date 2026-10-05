using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Panels;

/// <summary>Contract "panel": a modal with pools of pages, stats and rows, ids <c>p_*</c>. Takes the mouse.</summary>
public sealed class PanelLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public PanelLayout()
    {
        ExitSeconds = 0.2f;
    }

    public override string Component => "panel";

    /// <summary>Menu entries <c>p_page_0</c>..<c>p_page_{Pages-1}</c>.</summary>
    public int Pages { get; init; } = 8;

    /// <summary>Chips <c>p_stat_0</c>..<c>p_stat_{Stats-1}</c>.</summary>
    public int Stats { get; init; } = 4;

    /// <summary>Rows <c>p_row_0</c>..<c>p_row_{Rows-1}</c> of the selected page; the content scrolls.</summary>
    public int Rows { get; init; } = 24;
}
