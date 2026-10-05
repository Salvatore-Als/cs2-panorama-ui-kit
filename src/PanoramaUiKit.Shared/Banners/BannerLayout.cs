using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Banners;

/// <summary>Contract "banner": one card, ids <c>b_*</c>.</summary>
public sealed class BannerLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public BannerLayout()
    {
        ExitSeconds = 0.32f;
    }

    public override string Component => "banner";
}
