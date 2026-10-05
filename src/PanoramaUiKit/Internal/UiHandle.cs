using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Internal;

/// <summary>Handle whose state lives in the channel that issued it.</summary>
internal sealed class UiHandle : IUiHandle
{
    /// <summary>Returned when nothing could be shown.</summary>
    public static readonly UiHandle Dead = new(null);

    private bool _gone;
    private bool _cancelled;
    private IUiHandle? _inner;

    public UiHandle(CCSPlayerController? player)
    {
        Player = player;
        _gone = player == null;
    }

    public CCSPlayerController? Player { get; }

    /// <summary>Set by the channel; called by <see cref="Clear"/> while the handle is live.</summary>
    public Action? OnClear { get; set; }

    public bool IsVisible
    {
        get
        {
            if (_inner != null)
            {
                return _inner.IsVisible;
            }

            return !_gone && !_cancelled && Player is { IsValid: true };
        }
    }

    public void Clear()
    {
        if (_inner != null)
        {
            _inner.Clear();
            return;
        }

        if (!IsVisible)
        {
            return;
        }

        _cancelled = true;
        OnClear?.Invoke();
    }

    /// <summary>
    /// A handle handed out before the call it stands for could run (the layout was still warming up)
    /// follows the real one from now on. Cleared in the meantime, the real one is cleared at once.
    /// </summary>
    public void Link(IUiHandle inner)
    {
        _inner = inner;
        if (_cancelled)
        {
            inner.Clear();
        }
    }

    public void MarkGone()
    {
        _gone = true;
        OnClear = null;
    }
}
