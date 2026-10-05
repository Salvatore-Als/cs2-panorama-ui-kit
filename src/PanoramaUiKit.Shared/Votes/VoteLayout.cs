using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Votes;

/// <summary>
/// Contract "vote": one card, ids <c>vt_*</c>, a pool of <see cref="Choices"/> choice buttons. Takes the
/// mouse while open.
/// </summary>
public sealed class VoteLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public VoteLayout()
    {
        ExitSeconds = 0.35f;
    }

    public override string Component => "vote";

    /// <summary>Choice buttons per page: <c>vt_choice_0</c>..<c>vt_choice_{Choices-1}</c>. A longer vote is paged.</summary>
    public int Choices { get; init; } = 5;
}
