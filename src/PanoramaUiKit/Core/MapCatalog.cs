using System.Text.Json;
using PanoramaUiKit.Shared.Minimap;

namespace PanoramaUiKit.Core;

/// <summary>
/// How each map is drawn on the minimaps, by name, in three layers, the first found wins:
/// <list type="number">
/// <item>dev: set live from the dev menu (css_uikit_dev), saved to maps.json and loaded at start;</item>
/// <item>plugin: <c>IMinimapService.RegisterMap</c>;</item>
/// <item>official: <see cref="MinimapMaps.Official"/>.</item>
/// </list>
/// Dev wins so that a map calibrated in game stays as calibrated, whatever a plugin registered.
/// <see cref="Changed"/> fires on every change: the open maps are redrawn with it, live.
/// </summary>
internal sealed class MapCatalog
{
    private readonly Dictionary<string, MinimapMap> _dev = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MinimapMap> _plugin = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MinimapMap> _saved = new(StringComparer.OrdinalIgnoreCase);
    private string? _path;

    public event Action? Changed;

    public void Register(string name, MinimapMap map)
    {
        _plugin[name.Trim()] = map;
        Changed?.Invoke();
    }

    public void SetDev(string name, MinimapMap map)
    {
        _dev[name.Trim()] = map;
        Changed?.Invoke();
    }

    /// <summary>Drops the dev calibration of a map (and from maps.json at the next save).</summary>
    public bool ForgetDev(string name)
    {
        if (!_dev.Remove(name.Trim()))
        {
            return false;
        }

        Changed?.Invoke();
        return true;
    }

    public MinimapMap? Find(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        name = name.Trim();
        if (_dev.TryGetValue(name, out MinimapMap? dev))
        {
            return dev;
        }

        if (_plugin.TryGetValue(name, out MinimapMap? registered))
        {
            return registered;
        }

        return MinimapMaps.Official.TryGetValue(name, out MinimapBounds? bounds) ? new MinimapMap(name, bounds) : null;
    }

    /// <summary>Where <see cref="Find"/> gets this map from, for the dev menu.</summary>
    public string Source(string name)
    {
        name = name.Trim();
        if (_dev.TryGetValue(name, out MinimapMap? dev))
        {
            return _saved.TryGetValue(name, out MinimapMap? saved) && saved == dev ? "dev, sauvegardée" : "dev, NON sauvegardée";
        }

        if (_plugin.ContainsKey(name))
        {
            return "plugin";
        }

        return MinimapMaps.Official.ContainsKey(name) ? "officielle" : "inconnue";
    }

    private sealed record Entry(string Picture, float MinX, float MinY, float MaxX, float MaxY);

    /// <summary>Reads the dev calibrations saved in <paramref name="path"/>, if it exists. Remembered for <see cref="Save"/>.</summary>
    public void Load(string path)
    {
        _path = path;
        if (!File.Exists(path))
        {
            return;
        }

        Dictionary<string, Entry>? entries = JsonSerializer.Deserialize<Dictionary<string, Entry>>(File.ReadAllText(path));
        foreach ((string name, Entry e) in entries ?? [])
        {
            MinimapMap map = new(e.Picture, new MinimapBounds(e.MinX, e.MinY, e.MaxX, e.MaxY));
            _dev[name] = map;
            _saved[name] = map;
        }

        Changed?.Invoke();
    }

    /// <summary>Writes every dev calibration to the file <see cref="Load"/> read. Returns the path, null when there is none.</summary>
    public string? Save()
    {
        if (_path == null)
        {
            return null;
        }

        Dictionary<string, Entry> entries = _dev.ToDictionary(
            kv => kv.Key,
            kv => new Entry(kv.Value.Picture, kv.Value.Bounds.MinX, kv.Value.Bounds.MinY, kv.Value.Bounds.MaxX, kv.Value.Bounds.MaxY));
        File.WriteAllText(_path, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }));

        _saved.Clear();
        foreach ((string name, MinimapMap map) in _dev)
        {
            _saved[name] = map;
        }

        return _path;
    }
}
