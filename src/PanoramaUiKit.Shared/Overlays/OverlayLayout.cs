using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Overlays;

/// <summary>
/// An overlay layout: one full-screen effect panel, <c>ov_fx</c>, styled by <c>fx-&lt;name&gt;</c>. Contract "overlay"
/// (docs/CONTRACTS.md). Declare it once, reuse the instance.
/// </summary>
public sealed class OverlayLayout : UiLayout
{
    /// <summary>An effect has no entry or exit transition, so there is nothing to wait for.</summary>
    public OverlayLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "overlay";
}
