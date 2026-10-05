namespace PanoramaUiKit.Shared.KillFeed;

/// <summary>Server-wide feed, same rows for every human. Late joiners get it on the next push.</summary>
public interface IKillFeedService
{
    void Push(KillFeedEntry entry);

    /// <summary>Empties the feed and closes it for everyone. Null layout = every kill feed layout.</summary>
    void Clear(KillFeedLayout? layout = null);
}
