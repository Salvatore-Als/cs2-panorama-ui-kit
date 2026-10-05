using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Match;

/// <summary>
/// The top status bar, compact: a block on each side (a count and its title) around a centre (a timer
/// and a line under it), with an optional score tab hanging under each side. Same text for every
/// viewer. A missing value renders a dash.
///
/// <code>
///   [  6  ]   [  1:45  ]   [  6  ]
///   [  T  ]   [  Live  ]   [ CT  ]
///     [ 3 ]               [ 5 ]        &lt;- score tabs, only when ShowScore
/// </code>
/// </summary>
public sealed record MatchOptions : UiContent
{
    /// <summary>Title under the left count ("T", "Alive", a team name). Slot <c>{s:m_left_label}</c>.</summary>
    public string? LeftLabel { get; init; }

    /// <summary>The left count, big. Slot <c>{s:m_left_value}</c>.</summary>
    public string? LeftValue { get; init; }

    /// <summary>Colour of the left block and its score tab: <c>style-&lt;name&gt;</c> on <c>m_left</c> / <c>m_left_tab</c>.</summary>
    public string LeftStyle { get; init; } = UiStyle.Neutral;

    /// <summary>Left score, shown in the tab under the left block. Slot <c>{s:m_left_score}</c>.</summary>
    public int? LeftScore { get; init; }

    /// <summary>Slot <c>{s:m_center}</c>. See <see cref="FormatTime"/> for a clock.</summary>
    public string? Center { get; init; }

    /// <summary>Small line under the centre. Slot <c>{s:m_center_sub}</c>. Blank collapses it.</summary>
    public string? CenterSub { get; init; }

    /// <summary>Title under the right count. Slot <c>{s:m_right_label}</c>.</summary>
    public string? RightLabel { get; init; }

    /// <summary>The right count, big. Slot <c>{s:m_right_value}</c>.</summary>
    public string? RightValue { get; init; }

    /// <summary>Colour of the right block and its score tab: <c>style-&lt;name&gt;</c> on <c>m_right</c> / <c>m_right_tab</c>.</summary>
    public string RightStyle { get; init; } = UiStyle.Neutral;

    /// <summary>Right score, shown in the tab under the right block. Slot <c>{s:m_right_score}</c>.</summary>
    public int? RightScore { get; init; }

    /// <summary>
    /// Shows the score tabs (<c>m_scores</c> loses <c>hidden</c>). False hides them even when scores
    /// are given; they are also hidden when neither score is.
    /// </summary>
    public bool ShowScore { get; init; } = true;

    /// <summary>Class <c>urgent</c> on <c>m_bar</c>: last seconds, the timer turns red and pulses.</summary>
    public bool Urgent { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required MatchLayout Layout { get; init; }

    /// <summary><c>m:ss</c>; negative clamps to 0:00.</summary>
    public static string FormatTime(int totalSeconds)
    {
        int clamped = Math.Max(0, totalSeconds);
        return $"{clamped / 60}:{clamped % 60:D2}";
    }
}
