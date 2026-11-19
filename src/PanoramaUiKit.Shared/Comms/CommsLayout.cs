using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Comms;

/// <summary>A transmission layout: one card, <c>cm_card</c>, with its portrait <c>cm_face</c>. Contract "comms".</summary>
public sealed class CommsLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's power-off, plus a margin.</summary>
    public CommsLayout()
    {
        ExitSeconds = 0.3f;
    }

    public override string Component => "comms";
}
