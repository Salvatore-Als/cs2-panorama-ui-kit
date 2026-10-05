using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Internal;

namespace PanoramaUiKit.Channels.Base;

/// <summary>
/// One player's state on one layout: what their client shows (<see cref="Client"/>), the timers of
/// what is in flight, and the handle the caller holds. Starting a new phase cancels the timers of
/// the previous one, so a stale callback can never act on what replaced it.
/// </summary>
internal class ViewerState
{
    private readonly List<SafeTimer> _timers = [];
    private float _nextBeginAt;
    private float _lastAppliedAt = float.MinValue;

    public ViewerState(ClientState client)
    {
        Client = client;
    }

    public ClientState Client { get; }

    /// <summary>The handle of what is on screen now, if the component hands one out.</summary>
    public UiHandle? Handle { get; private set; }

    /// <summary>Issues a fresh handle; the previous one goes dead.</summary>
    public UiHandle NewHandle(CCSPlayerController player, Action onClear)
    {
        Handle?.MarkGone();
        Handle = new UiHandle(player) { OnClear = onClear };
        return Handle;
    }

    public void ReleaseHandle()
    {
        Handle?.MarkGone();
        Handle = null;
    }

    /// <summary>
    /// Books the next slot of a queue: how long to wait before this write may happen so that writes
    /// are at least <paramref name="gap"/> apart. Every call books a later slot (a burst of five
    /// becomes five writes spread over five gaps). For things that must all be shown, like toasts.
    /// </summary>
    public float Reserve(float now, float gap)
    {
        float start = Math.Max(now, _nextBeginAt);
        _nextBeginAt = start + gap;
        return start - now;
    }

    /// <summary>
    /// How long to wait so that this write comes at least <paramref name="gap"/> after the last one
    /// that was <see cref="MarkApplied"/>. Does not book anything: a caller that replaces its pending
    /// write (the latest content wins) gets the same delay every time.
    /// </summary>
    public float PaceDelay(float now, float gap) => Math.Max(0f, _lastAppliedAt + gap - now);

    public void MarkApplied(float now) => _lastAppliedAt = now;

    public SafeTimer Track(SafeTimer timer)
    {
        _timers.RemoveAll(t => t.Fired);
        _timers.Add(timer);
        return timer;
    }

    public void CancelTimers()
    {
        foreach (SafeTimer timer in _timers)
        {
            timer.Kill();
        }

        _timers.Clear();
    }

    /// <summary>Called when the viewer is dropped. Overrides must call base.</summary>
    public virtual void Release()
    {
        CancelTimers();
        ReleaseHandle();
    }
}
