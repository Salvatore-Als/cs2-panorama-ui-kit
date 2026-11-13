namespace PanoramaUiKit.Shared.Recap;

/// <summary>
/// One stat tile on the recap strip. Slots <c>{s:rc_stat_&lt;n&gt;_value}</c> and <c>{s:rc_stat_&lt;n&gt;_label}</c>;
/// <see cref="Icon"/> is <c>icon-&lt;name&gt;</c> on <c>rc_stat_&lt;n&gt;_icon</c> (see <see cref="RecapIcons"/>),
/// <see cref="Style"/> is <c>style-*</c> on <c>rc_stat_&lt;n&gt;</c>, <see cref="Best"/> adds <c>best</c> on it.
/// <see cref="RecapStats"/> builds the common ones.
/// </summary>
public sealed record RecapStat(string Icon, string Value, string Label, string Style = "neutral", bool Best = false);
