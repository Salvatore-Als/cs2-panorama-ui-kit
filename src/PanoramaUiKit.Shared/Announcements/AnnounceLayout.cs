using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Announcements;

/// <summary>Contract "announce": one card, ids <c>a_*</c>.</summary>
public sealed class AnnounceLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public AnnounceLayout()
    {
        ExitSeconds = 0.36f;
    }

    public override string Component => "announce";
}
