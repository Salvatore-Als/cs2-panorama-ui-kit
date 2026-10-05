using PanoramaManager;
using PanoramaUiKit.Internal;

namespace PanoramaUiKit.Channels.Base;

/// <summary>
/// Text that is the same for every viewer (match bar, kill feed): written once through the shared
/// setter, which PanoramaManager replays to whoever opens the layout later, and only when it changed.
/// </summary>
internal sealed class SharedText
{
    private readonly PanelHandle _panel;
    private readonly Dictionary<string, string> _written = new();
    private readonly Dictionary<string, List<string>> _extraKeys = new();

    public SharedText(PanelHandle panel)
    {
        _panel = panel;
    }

    public void Write(string name, string value)
    {
        if (_written.TryGetValue(name, out string? current) && current == value)
        {
            return;
        }

        _written[name] = value;
        _panel.SetVariable(name, value);
    }

    /// <summary><c>UiContent.Texts</c> as <c>{prefix}_{key}</c>; stale keys of the same prefix are blanked.</summary>
    public void ExtraTexts(string prefix, IReadOnlyDictionary<string, string>? texts)
    {
        List<string> keys = [];
        foreach ((string key, string value) in Extras.Normalize(texts))
        {
            keys.Add(key);
            Write($"{prefix}_{key}", value);
        }

        if (_extraKeys.TryGetValue(prefix, out List<string>? previous))
        {
            foreach (string stale in previous.Where(k => !keys.Contains(k)))
            {
                Write($"{prefix}_{stale}", string.Empty);
            }
        }

        _extraKeys[prefix] = keys;
    }
}
