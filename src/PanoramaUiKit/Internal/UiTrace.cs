using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace PanoramaUiKit.Internal;

/// <summary>
/// Optional log of everything the kit sends to a client (<c>css_uikit_trace on</c>): one line per
/// text or class write, with the server tick, so a burst that overflows a client's connection shows
/// as many lines on one tick. Off by default; costs nothing while off.
/// </summary>
internal static class UiTrace
{
    private static ILogger? _logger;
    private static int _lastTick = -1;
    private static int _writesThisTick;

    public static bool Enabled { get; set; }

    public static void Attach(ILogger logger) => _logger = logger;

    public static void Write(string layout, string kind, string panelId, string detail)
    {
        if (!Enabled || _logger == null)
        {
            return;
        }

        int tick = Server.TickCount;
        if (tick != _lastTick)
        {
            if (_writesThisTick > 0)
            {
                _logger.LogInformation("[trace] tick {Tick}: {Count} write(s)", _lastTick, _writesThisTick);
            }

            _lastTick = tick;
            _writesThisTick = 0;
        }

        _writesThisTick++;
        _logger.LogInformation("[trace] tick {Tick} {Layout} {Kind} {Panel} {Detail}", tick, layout, kind, panelId, detail);
    }

    /// <summary>A call that held the server thread for a noticeable time. Logged even while the trace is off.</summary>
    public static void Slow(string layout, string what, double milliseconds)
    {
        if (_logger != null && milliseconds >= 5)
        {
            _logger.LogWarning("[slow] {Layout} {What} took {Ms:F1} ms", layout, what, milliseconds);
        }
    }

    /// <summary>Layout entity lifecycle: spawn, open, close.</summary>
    public static void Event(string layout, string what)
    {
        if (Enabled && _logger != null)
        {
            _logger.LogInformation("[trace] tick {Tick} {Layout} {What}", Server.TickCount, layout, what);
        }
    }
}
