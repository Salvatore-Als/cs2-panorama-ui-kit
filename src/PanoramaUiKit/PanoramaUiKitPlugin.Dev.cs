using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Menus;
using PanoramaUiKit.Shared.Minimap;

namespace PanoramaUiKit;

/// <summary>
/// The dev environment: css_uikit_dev opens a menu to try things live, without a restart. Its Map section calibrates
/// the current map's minimap: which picture, which world rectangle it covers. Every change is drawn at once on every
/// open map (yours and every plugin's), "Sauvegarder" writes it to maps.json next to the plugin, loaded at every start,
/// and "Imprimer le code" gives the RegisterMap line to paste in a plugin instead.
/// </summary>
public sealed partial class PanoramaUiKitPlugin
{
    private const string DevMapsFile = "maps.json";

    private static readonly MenuLayout DevMenuLayout = new() { Name = "uikit_menu" };
    private static readonly MinimapLayout DevMinimap = new() { Name = "uikit_minimap" };
    private static readonly MinimapLayout DevBigMap = new() { Name = "uikit_bigmap" };

    private static readonly float[] DevSteps = [10f, 50f, 250f, 1000f];

    /// <summary>Landmarks dropped from the dev menu: the last marker slots, on every minimap layout.</summary>
    private const int DevFirstMark = 48;

    private const int DevMarks = 16;

    /// <summary>A map nobody knows starts as this square around the player, to be narrowed with the corners.</summary>
    private const float DevDefaultSide = 8192f;

    private int _devStep = 1;
    private int _devMarkCount;

    private void LoadDevMaps(LayoutRegistry registry)
    {
        string path = Path.Combine(ModuleDirectory, DevMapsFile);
        try
        {
            registry.Maps.Load(path);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Panorama UI Kit: {Path} could not be read; the dev map calibrations are not loaded.", path);
        }
    }

    [ConsoleCommand("css_uikit_dev", "Opens the Panorama UI Kit dev menu: live map calibration and more.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDev(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Allowed(caller, command) || _kit == null || caller is not { IsValid: true })
        {
            return;
        }

        // Straight to the Map page: calibrating means closing the menu, walking to a corner and coming back.
        _kit.Menus.Open(caller, DevRoot());
        _kit.Menus.Push(caller, DevMapMenu(caller));
    }

    private MenuOptions DevRoot() => DevPage("Panorama UI Kit · dev", "Tout se règle en direct, sans redémarrer",
    [
        MenuEntry.Open("Map", DevMapMenu, "Calibrer la minimap de la map en cours : image, limites, repères, code."),
        DevButton("Tout effacer de mon écran", p => _kit!.ClearAll(p)),
    ]);

    // Map -------------------------------------------------------------------------------------------

    private MenuOptions DevMapMenu(CCSPlayerController player)
    {
        string map = Server.MapName;
        MinimapMap current = DevCurrent(player);
        MinimapBounds b = current.Bounds;
        return DevPage("Map", $"{map} · {current.Picture} · {_registry!.Maps.Source(map)}",
        [
            DevInfo("Ma position", DevWorld(player), "Clique pour rafraîchir."),
            DevInfo("Sur l'image", DevOnPicture(player, b), "Où tu es sur l'image, depuis le coin haut-gauche, et la case envoyée."),
            DevInfo("Limites X", $"{b.MinX:0} .. {b.MaxX:0} ({b.Width:0})", "Gauche .. droite de l'image, en unités du jeu."),
            DevInfo("Limites Y", $"{b.MaxY:0} .. {b.MinY:0} ({b.Height:0})", "Haut .. bas de l'image, en unités du jeu."),
            MenuEntry.Open("Image", DevImageMenu, "Quelle image dessiner : radar officiel ou slot 1 à 32."),
            MenuEntry.Open("Coins", DevCornersMenu, "Poser les coins de l'image là où tu te tiens."),
            MenuEntry.Open("Ajuster", DevAdjustMenu, "Décaler, agrandir, réduire, au pas choisi."),
            MenuEntry.Open("Afficher et code", DevShowMenu, "Cartes, repères, code à coller, sauvegarde."),
        ]);
    }

    private MenuOptions DevImageMenu(CCSPlayerController player)
    {
        MinimapMap current = DevCurrent(player);
        List<string> pictures = DevPictures();
        int index = Math.Max(0, pictures.IndexOf(current.Picture));
        return DevPage("Image", $"{Server.MapName} · {current.Picture}",
        [
            DevInfo("Image", current.Picture, "map-<image> sur la carte : un slot rempli par ton addon, ou le radar officiel."),
            DevEdit("Précédente", DevImageMenu, p => DevCurrent(p) with { Picture = pictures[(index - 1 + pictures.Count) % pictures.Count] }),
            DevEdit("Suivante", DevImageMenu, p => DevCurrent(p) with { Picture = pictures[(index + 1) % pictures.Count] }),
            DevEdit("Slot 1", DevImageMenu, p => DevCurrent(p) with { Picture = MinimapMaps.Slot(1) },
                    "panorama/images/custom_game/uikit/minimap/maps/slot_1.png dans ton addon."),
        ]);
    }

    private MenuOptions DevCornersMenu(CCSPlayerController player) => DevPage("Coins", DevBoundsLine(player),
    [
        DevEdit("Coin haut-gauche = ici", DevCornersMenu, p => DevPosition(p) is { } at
                    ? DevWith(p, b => new MinimapBounds(at.X, b.MinY, b.MaxX, at.Y))
                    : null,
                "Va au coin haut-gauche de la zone que montre l'image."),
        DevEdit("Coin bas-droit = ici", DevCornersMenu, p => DevPosition(p) is { } at
                    ? DevWith(p, b => new MinimapBounds(b.MinX, at.Y, at.X, b.MaxY))
                    : null,
                "Va au coin bas-droit."),
        DevEdit("Rendre carré", DevCornersMenu, p => DevWith(p, b =>
                {
                    float side = Math.Max(b.Width, b.Height) / 2f;
                    (float cx, float cy) = ((b.MinX + b.MaxX) / 2f, (b.MinY + b.MaxY) / 2f);
                    return new MinimapBounds(cx - side, cy - side, cx + side, cy + side);
                }),
                "Le plus grand côté, autour du même centre : l'image est carrée, les limites doivent l'être."),
        DevEdit("Centrer sur moi", DevCornersMenu, p => DevPosition(p) is { } at
                    ? DevWith(p, b => new MinimapBounds(at.X - b.Width / 2f, at.Y - b.Height / 2f, at.X + b.Width / 2f, at.Y + b.Height / 2f))
                    : null,
                "Même taille, centrée là où tu es."),
    ]);

    private MenuOptions DevAdjustMenu(CCSPlayerController player)
    {
        float step = DevSteps[_devStep];
        return DevPage("Ajuster", DevBoundsLine(player),
        [
            MenuEntry.Select("Pas", DevSteps.Select(s => s.ToString("0", CultureInfo.InvariantCulture)).ToList(), _devStep,
                             (p, i, _) => { _devStep = i; DevRefresh(p, DevAdjustMenu); }, "Unités du jeu par clic."),
            DevEdit($"Limites vers la gauche (X -{step:0})", DevAdjustMenu, p => DevWith(p, b => Shift(b, -step, 0f)),
                    "Les points se décalent vers la droite sur l'image."),
            DevEdit($"Limites vers la droite (X +{step:0})", DevAdjustMenu, p => DevWith(p, b => Shift(b, step, 0f)),
                    "Les points se décalent vers la gauche sur l'image."),
            DevEdit($"Limites vers le haut (Y +{step:0})", DevAdjustMenu, p => DevWith(p, b => Shift(b, 0f, step)),
                    "Les points descendent sur l'image."),
            DevEdit($"Limites vers le bas (Y -{step:0})", DevAdjustMenu, p => DevWith(p, b => Shift(b, 0f, -step)),
                    "Les points montent sur l'image."),
            DevEdit($"Agrandir ({step * 2:0})", DevAdjustMenu, p => DevWith(p, b => Grow(b, step)),
                    "L'image couvre plus de monde : les points se resserrent vers le centre."),
            DevEdit($"Réduire ({step * 2:0})", DevAdjustMenu, p => DevWith(p, b => Grow(b, -step)),
                    "L'image couvre moins de monde : les points s'écartent du centre."),
        ]);

        static MinimapBounds Shift(MinimapBounds b, float dx, float dy) => new(b.MinX + dx, b.MinY + dy, b.MaxX + dx, b.MaxY + dy);

        static MinimapBounds Grow(MinimapBounds b, float by) =>
            b.Width + 2 * by <= 0f || b.Height + 2 * by <= 0f ? b : new(b.MinX - by, b.MinY - by, b.MaxX + by, b.MaxY + by);
    }

    private MenuOptions DevShowMenu(CCSPlayerController player) => DevPage("Afficher et code", $"{Server.MapName} · {_registry!.Maps.Source(Server.MapName)}",
    [
        DevButton("Mini map on / off", p => DevToggleMap(p, DevMinimap)),
        DevButton("Grosse carte on / off", p => DevToggleMap(p, DevBigMap)),
        DevButton("Repère ici", DevDropMark, "Un point rose numéroté là où tu es, sur toutes les cartes, pour comparer avec l'image."),
        DevButton("Effacer les repères", _ => DevClearMarks()),
        DevButton("Imprimer le code", DevPrintCode, "La ligne RegisterMap à coller dans ton plugin (console et chat)."),
        DevButton("Sauvegarder", DevSave, $"Dans {DevMapsFile}, à côté du plugin : rechargé à chaque démarrage."),
        DevButton("Oublier la calibration dev", p =>
        {
            bool forgot = _registry!.Maps.ForgetDev(Server.MapName);
            p.PrintToChat(forgot ? "[uikit] calibration dev oubliée (sauvegarde pour l'enlever du fichier)" : "[uikit] aucune calibration dev sur cette map");
        }, "Revient à la version plugin ou officielle."),
    ]);

    // Edits -----------------------------------------------------------------------------------------

    /// <summary>The current map's calibration: the catalog's, else a square around the player on slot 1.</summary>
    private MinimapMap DevCurrent(CCSPlayerController player)
    {
        if (_registry!.Maps.Find(Server.MapName) is { } known)
        {
            return known;
        }

        (float x, float y) = DevPosition(player) ?? (0f, 0f);
        float half = DevDefaultSide / 2f;
        return new MinimapMap(MinimapMaps.Slot(1), new MinimapBounds(x - half, y - half, x + half, y + half));
    }

    private MinimapMap DevWith(CCSPlayerController player, Func<MinimapBounds, MinimapBounds> change)
    {
        MinimapMap current = DevCurrent(player);
        return current with { Bounds = change(current.Bounds) };
    }

    /// <summary>A button that sets the current map's calibration to what <paramref name="next"/> returns (null = nothing).</summary>
    private MenuEntry DevEdit(string label, Func<CCSPlayerController, MenuOptions> page, Func<CCSPlayerController, MinimapMap?> next,
                              string? description = null) =>
        DevButton(label, p =>
        {
            if (next(p) is { } map)
            {
                _registry!.Maps.SetDev(Server.MapName, map);
            }

            DevRefresh(p, page);
        }, description);

    private List<string> DevPictures()
    {
        List<string> pictures = [];
        if (MinimapMaps.Official.ContainsKey(Server.MapName))
        {
            pictures.Add(Server.MapName.ToLowerInvariant());
        }

        pictures.AddRange(Enumerable.Range(1, MinimapMaps.Slots).Select(MinimapMaps.Slot));
        return pictures;
    }

    // Show and code ---------------------------------------------------------------------------------

    private void DevToggleMap(CCSPlayerController player, MinimapLayout layout)
    {
        if (_kit!.Minimap.IsShowing(player, layout))
        {
            _kit.Minimap.Hide(player, layout);
            return;
        }

        _kit.Minimap.Show(player, new MinimapOptions { Layout = layout, Title = Server.MapName });
    }

    private void DevDropMark(CCSPlayerController player)
    {
        if (DevPosition(player) is not { } at)
        {
            return;
        }

        int n = _devMarkCount++;
        _kit!.Minimap.SetGlobalMark(null, new MinimapMark(DevFirstMark + n % DevMarks, MinimapNames.Dot, at.X, at.Y,
                                                         Label: (n + 1).ToString(CultureInfo.InvariantCulture), Style: MinimapColors.Pink));
        player.PrintToChat($"[uikit] repère {n + 1} à ({at.X:0}, {at.Y:0})");
    }

    private void DevClearMarks()
    {
        for (int i = 0; i < DevMarks; i++)
        {
            _kit!.Minimap.ClearGlobalMark(null, DevFirstMark + i);
        }

        _devMarkCount = 0;
    }

    private void DevPrintCode(CCSPlayerController player)
    {
        string map = Server.MapName;
        MinimapMap current = DevCurrent(player);
        MinimapBounds b = current.Bounds;
        string picture = current.Picture.StartsWith("slot-", StringComparison.Ordinal)
            ? $"MinimapMaps.Slot({current.Picture["slot-".Length..]})"
            : $"\"{current.Picture}\"";
        string bounds = string.Create(CultureInfo.InvariantCulture, $"new MinimapBounds({b.MinX:0.##}f, {b.MinY:0.##}f, {b.MaxX:0.##}f, {b.MaxY:0.##}f)");
        string line = $"kit.Minimap.RegisterMap(\"{map}\", new MinimapMap({picture}, {bounds}));";
        string overview = string.Create(CultureInfo.InvariantCulture,
            $"overview: pos_x {b.MinX:0.##}, pos_y {b.MaxY:0.##}, scale {b.Width / 1024f:0.####} (radar de 1024px)");

        player.PrintToConsole(line);
        player.PrintToConsole(overview);
        if (Math.Abs(b.Width - b.Height) > 1f)
        {
            player.PrintToConsole($"attention : limites non carrées ({b.Width:0} x {b.Height:0}), l'image carrée sera déformée");
        }

        player.PrintToChat("[uikit] code imprimé dans ta console");
    }

    private void DevSave(CCSPlayerController player)
    {
        try
        {
            string? path = _registry!.Maps.Save();
            player.PrintToChat(path == null ? "[uikit] pas de fichier de sauvegarde" : $"[uikit] sauvegardé dans {DevMapsFile}");
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Panorama UI Kit: the dev map calibrations could not be saved.");
            player.PrintToChat("[uikit] sauvegarde impossible, voir le log du serveur");
        }
    }

    // Helpers ---------------------------------------------------------------------------------------

    private static (float X, float Y)? DevPosition(CCSPlayerController player) =>
        player.PlayerPawn.Value?.AbsOrigin is { } at ? (at.X, at.Y) : null;

    private static string DevWorld(CCSPlayerController player) =>
        DevPosition(player) is { } at ? string.Create(CultureInfo.InvariantCulture, $"{at.X:0}, {at.Y:0}") : "pas de pawn";

    private static string DevOnPicture(CCSPlayerController player, MinimapBounds b)
    {
        if (DevPosition(player) is not { } at)
        {
            return "-";
        }

        (float u, float v) = b.Normalize(at.X, at.Y);
        if (u is < 0f or > 1f || v is < 0f or > 1f)
        {
            return string.Create(CultureInfo.InvariantCulture, $"HORS de l'image ({u * 100f:0}%, {v * 100f:0}%)");
        }

        int grid = DevMinimap.Grid;
        return string.Create(CultureInfo.InvariantCulture,
            $"{u * 100f:0.#}% · {v * 100f:0.#}% (case {(int)(u * grid)}, {(int)(v * grid)})");
    }

    private string DevBoundsLine(CCSPlayerController player)
    {
        MinimapBounds b = DevCurrent(player).Bounds;
        return string.Create(CultureInfo.InvariantCulture, $"X {b.MinX:0}..{b.MaxX:0} · Y {b.MaxY:0}..{b.MinY:0}");
    }

    private static MenuOptions DevPage(string title, string subtitle, IReadOnlyList<MenuEntry> entries) => new()
    {
        Layout = DevMenuLayout,
        Title = title,
        Subtitle = subtitle,
        Style = UiStyle.Info,
        Entries = entries,
    };

    private MenuEntry DevInfo(string label, string value, string? description = null) => new()
    {
        Kind = MenuEntryKind.Button,
        Label = label,
        Value = value,
        Description = description,
        OnClick = p => DevRefresh(p, DevMapMenu, top: true),
    };

    /// <summary>A button run next frame, out of the menu's click handler.</summary>
    private static MenuEntry DevButton(string label, Action<CCSPlayerController> action, string? description = null) =>
        MenuEntry.Button(label, p => Server.NextFrame(() =>
        {
            if (p.IsValid)
            {
                action(p);
            }
        }), description);

    /// <summary>
    /// Shows the page again with fresh values, and the Map page under it: a menu keeps the values it was opened with.
    /// From a sub-page: back twice, then Map and the page again. From the Map page itself (<paramref name="top"/>): once.
    /// </summary>
    private void DevRefresh(CCSPlayerController player, Func<CCSPlayerController, MenuOptions> page, bool top = false)
    {
        Server.NextFrame(() =>
        {
            if (!player.IsValid || _kit == null)
            {
                return;
            }

            _kit.Menus.Back(player);
            if (!top)
            {
                _kit.Menus.Back(player);
                _kit.Menus.Push(player, DevMapMenu(player));
            }

            _kit.Menus.Push(player, page(player));
        });
    }
}
