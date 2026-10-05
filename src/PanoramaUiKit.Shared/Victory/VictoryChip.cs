namespace PanoramaUiKit.Shared.Victory;

/// <summary>
/// A small card on the victory screen (MVP, best player, a stat...). Slots
/// <c>{s:v_chip_&lt;n&gt;_label}</c> and <c>{s:v_chip_&lt;n&gt;_name}</c>; <see cref="Style"/> is
/// <c>style-*</c> on <c>v_chip_&lt;n&gt;</c>.
/// </summary>
public sealed record VictoryChip(string Label, string Name, string Style = "neutral");
