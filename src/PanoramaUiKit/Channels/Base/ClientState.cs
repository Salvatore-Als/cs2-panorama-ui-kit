using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Internal;

namespace PanoramaUiKit.Channels.Base;

/// <summary>
/// What one viewer's client shows of a layout, so a repaint only sends what changed: text slots,
/// classes, class groups (one of <c>style-*</c>), free class sets and extra text slots.
///
/// <para>Without an <c>authoredOn</c> rule a class never written is assumed off, so its first "off"
/// is not sent (every send is a network update; a burst of them overflows the client). "hidden" is
/// the exception: pooled panels are authored hidden, so it is always written. Interactive layouts,
/// which repaint everything on every click, pass the rule and a long-lived <c>touched</c> set: a
/// class still in its authored state is skipped, unless this slot ever moved it (nothing guarantees
/// a close scrubs per-player classes).</para>
/// </summary>
internal sealed class ClientState
{
    private readonly PanelHandle _panel;
    private readonly string _layoutName;
    private readonly Func<string, string, bool>? _authoredOn;
    private readonly HashSet<(string Panel, string Class)>? _touched;
    private readonly Dictionary<string, string> _texts = new();
    private readonly Dictionary<(string Panel, string Class), bool> _classes = new();
    private readonly Dictionary<string, string?> _groups = new();
    private readonly Dictionary<string, List<string>> _freeClasses = new();
    private readonly Dictionary<string, List<string>> _extraKeys = new();

    public ClientState(PanelHandle panel, string layoutName, Func<string, string, bool>? authoredOn = null,
                       HashSet<(string, string)>? touched = null)
    {
        _panel = panel;
        _layoutName = layoutName;
        _authoredOn = authoredOn;
        _touched = touched;
    }

    /// <summary>The client was rebuilt from the layout (round restart): forget everything.</summary>
    public void Reset()
    {
        _texts.Clear();
        _classes.Clear();
        _groups.Clear();
        _freeClasses.Clear();
        _extraKeys.Clear();
        _touched?.Clear();
    }

    public void Text(CCSPlayerController player, string name, string value)
    {
        if (_texts.TryGetValue(name, out string? current) && current == value)
        {
            return;
        }

        _texts[name] = value;
        UiTrace.Write(_layoutName, "text", name, value);
        _panel.SetVariableFor(player, name, value);
    }

    /// <summary>A text slot whose label collapses when blank: <c>empty</c> on the panel with the same id.</summary>
    public void OptionalText(CCSPlayerController player, string name, string? value)
    {
        string text = value?.Trim() ?? string.Empty;
        Text(player, name, text);
        Class(player, name, "empty", text.Length == 0);
    }

    public void Class(CCSPlayerController player, string panelId, string className, bool on)
    {
        (string, string) key = (panelId, className);
        bool skip;
        if (_classes.TryGetValue(key, out bool current))
        {
            skip = current == on;
        }
        else if (_authoredOn != null)
        {
            skip = on == _authoredOn(panelId, className) && _touched?.Contains(key) != true;
        }
        else
        {
            skip = !on && className != "hidden";
        }

        if (skip)
        {
            return;
        }

        _classes[key] = on;
        UiTrace.Write(_layoutName, "class", panelId, $"{className}={on}");
        _panel.SetClassFor(player, panelId, className, on);
        if (_authoredOn != null && on != _authoredOn(panelId, className))
        {
            _touched?.Add(key);
        }
    }

    /// <summary>One class of a named group on a panel (style-*, anim-*): the previous one off, the new one on.</summary>
    public void Group(CCSPlayerController player, string panelId, string group, string? next)
    {
        string key = $"{panelId}|{group}";
        _groups.TryGetValue(key, out string? current);
        if (current != null && current != next)
        {
            Class(player, panelId, current, false);
        }

        if (next != null)
        {
            Class(player, panelId, next, true);
        }

        _groups[key] = next;
    }

    /// <summary>A free set of classes on a panel (<c>UiContent.Classes</c>): the ones dropped go off.</summary>
    public void FreeClasses(CCSPlayerController player, string panelId, IReadOnlyList<string>? requested)
    {
        List<string> next = Names.Classes(requested);
        if (!_freeClasses.TryGetValue(panelId, out List<string>? current))
        {
            current = [];
        }

        foreach (string stale in current.Where(c => !next.Contains(c)))
        {
            Class(player, panelId, stale, false);
        }

        foreach (string cls in next)
        {
            Class(player, panelId, cls, true);
        }

        _freeClasses[panelId] = next;
    }

    /// <summary>
    /// <c>UiContent.Texts</c> as <c>{prefix}_{key}</c>, collapsible. Keys the previous content wrote
    /// and this one does not are blanked, so a reused slot never shows stale extra text.
    /// </summary>
    public void ExtraTexts(CCSPlayerController player, string prefix, IReadOnlyDictionary<string, string>? texts)
    {
        List<string> keys = [];
        foreach ((string key, string value) in Extras.Normalize(texts))
        {
            keys.Add(key);
            OptionalText(player, $"{prefix}_{key}", value);
        }

        if (_extraKeys.TryGetValue(prefix, out List<string>? previous))
        {
            foreach (string stale in previous.Where(k => !keys.Contains(k)))
            {
                OptionalText(player, $"{prefix}_{stale}", null);
            }
        }

        _extraKeys[prefix] = keys;
    }
}
