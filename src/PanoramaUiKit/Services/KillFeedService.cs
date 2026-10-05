using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.KillFeed;

namespace PanoramaUiKit.Services;

internal sealed class KillFeedService(LayoutRegistry registry) : ComponentService<KillFeedChannel, KillFeedLayout>(registry), IKillFeedService
{
    public void Push(KillFeedEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.Victim))
        {
            WhenWarm(Channel(entry.Layout), channel => channel.Push(entry));
        }
    }

    public void Clear(KillFeedLayout? layout = null)
    {
        foreach (KillFeedChannel channel in Targets(layout))
        {
            channel.Clear();
        }
    }
}
