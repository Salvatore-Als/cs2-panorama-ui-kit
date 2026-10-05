using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Panels;

/// <summary>
/// A key / value chip above the rows of a page. Slots <c>{s:p_stat_&lt;n&gt;_key}</c> and
/// <c>{s:p_stat_&lt;n&gt;_value}</c>; <see cref="Style"/> is <c>style-*</c> on <c>p_stat_&lt;n&gt;</c>.
/// A blank key collapses the key line (class <c>no-key</c>). A page without stats hides the whole
/// strip (<c>hidden</c> on <c>p_stats</c>).
/// </summary>
public sealed record PanelStat(string Key, string Value, string Style = UiStyle.Neutral);
