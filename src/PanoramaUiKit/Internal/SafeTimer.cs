using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;

namespace PanoramaUiKit.Internal;

/// <summary>
/// One-shot timer whose <see cref="Kill"/> never reaches the native timer system.
///
/// <para>CounterStrikeSharp runs one-shot timers in a loop that erases the executed timer BY INDEX
/// after its callback. Killing another one-shot timer from inside a one-shot callback shifts that
/// list, the wrong entry is erased, and the next frame reads freed memory: a segfault with no .NET
/// stack. So Kill only cancels; the native timer still elapses and runs a callback that does nothing.</para>
/// </summary>
internal sealed class SafeTimer
{
    private bool _cancelled;

    private SafeTimer()
    {
    }

    public static SafeTimer Once(BasePlugin plugin, float delay, Action callback)
    {
        SafeTimer timer = new SafeTimer();
        plugin.AddTimer(delay, () =>
        {
            if (timer._cancelled)
            {
                return;
            }

            timer._cancelled = true;
            callback();
        }, TimerFlags.STOP_ON_MAPCHANGE);

        return timer;
    }

    /// <summary>Ran or was killed: nothing left to cancel.</summary>
    public bool Fired => _cancelled;

    public void Kill()
    {
        _cancelled = true;
    }
}
