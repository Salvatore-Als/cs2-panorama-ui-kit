using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Screamers;

/// <summary>A screamer layout: one full-screen picture, <c>sc_image</c>. Contract "screamer".</summary>
public sealed class ScreamerLayout : UiLayout
{
    public ScreamerLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "screamer";
}
