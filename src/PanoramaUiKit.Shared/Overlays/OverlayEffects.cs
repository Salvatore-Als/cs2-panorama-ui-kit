// Written by tools/overlay_fx.py from its EFFECTS list. Do not edit: edit the list and run it.
namespace PanoramaUiKit.Shared.Overlays;

/// <summary>
/// The full-screen effects the bundled overlay stylesheet has a rule for: <c>fx-&lt;name&gt;</c> on <c>ov_fx</c>
/// (<see cref="IOverlayService.Show"/>). A custom layout can add its own names.
/// </summary>
public static class OverlayEffects
{
    /// <summary>Givre aux coins.</summary>
    public const string Frozen1 = "frozen1";

    /// <summary>Cadre de glace : bord intérieur bleu glace et liseré blanc.</summary>
    public const string Frozen2 = "frozen2";

    /// <summary>Éclats de glace plantés sur les bords.</summary>
    public const string Frozen3 = "frozen3";

    /// <summary>Buée : souffle blanc qui monte du bas et respire.</summary>
    public const string Frozen4 = "frozen4";

    /// <summary>Froid : monde bleuté, bords bleu nuit.</summary>
    public const string Frozen5 = "frozen5";

    /// <summary>Givre qui gagne : avance des coins vers le centre.</summary>
    public const string Frozen6 = "frozen6";

    /// <summary>Neige qui tombe devant la vue.</summary>
    public const string Frozen7 = "frozen7";

    /// <summary>Vitre gelée : monde flou derrière un verre blanchi.</summary>
    public const string Frozen8 = "frozen8";

    /// <summary>Ralenti : bandes bleu nuit, monde bleuté.</summary>
    public const string Frozen9 = "frozen9";

    /// <summary>Coup de froid : flash bleu-blanc puis givre.</summary>
    public const string Frozen10 = "frozen10";

    /// <summary>Hypothermie : pulsation bleue lente.</summary>
    public const string Frozen11 = "frozen11";

    /// <summary>Stalactites qui pendent du haut.</summary>
    public const string Frozen12 = "frozen12";

    /// <summary>Craquelures de glace devant les yeux.</summary>
    public const string Frozen13 = "frozen13";

    /// <summary>Aurore : voile vert-cyan qui ondule en haut.</summary>
    public const string Frozen14 = "frozen14";

    /// <summary>Engourdi : vignette bleu nuit lourde.</summary>
    public const string Frozen15 = "frozen15";

    /// <summary>Scintillement : paillettes de glace.</summary>
    public const string Frozen16 = "frozen16";

    /// <summary>Banquise : glace qui monte du bas.</summary>
    public const string Frozen17 = "frozen17";

    /// <summary>Blizzard : écran blanchi, neige rapide.</summary>
    public const string Frozen18 = "frozen18";

    /// <summary>Cryo néon : contour bleu glace qui pulse.</summary>
    public const string Frozen19 = "frozen19";

    /// <summary>Gelé : teinte bleue, givre et glace aux coins.</summary>
    public const string Frozen20 = "frozen20";

    /// <summary>Flammes qui lèchent le bas de l'écran.</summary>
    public const string Burn1 = "burn1";

    /// <summary>Bord incandescent.</summary>
    public const string Burn2 = "burn2";

    /// <summary>Braises qui montent et s'éteignent.</summary>
    public const string Burn3 = "burn3";

    /// <summary>Chaleur : monde flou et orangé.</summary>
    public const string Burn4 = "burn4";

    /// <summary>Fumée grise qui passe.</summary>
    public const string Burn5 = "burn5";

    /// <summary>Bords carbonisés, liseré rougeoyant.</summary>
    public const string Burn6 = "burn6";

    /// <summary>Embrasement : flash orange puis voile rouge.</summary>
    public const string Burn7 = "burn7";

    /// <summary>Cendres qui tombent lentement.</summary>
    public const string Burn8 = "burn8";

    /// <summary>Feu au cœur : pulsation rouge-orange rapide.</summary>
    public const string Burn9 = "burn9";

    /// <summary>Calciné : monde brun-noir, grain de suie.</summary>
    public const string Burn10 = "burn10";

    /// <summary>Les deux côtés flambent.</summary>
    public const string Burn11 = "burn11";

    /// <summary>Lueur orange additive.</summary>
    public const string Burn12 = "burn12";

    /// <summary>Napalm : monde orangé, feu jaune au sol.</summary>
    public const string Burn13 = "burn13";

    /// <summary>Étincelles vives et rapides.</summary>
    public const string Burn14 = "burn14";

    /// <summary>Fumée noire qui gonfle.</summary>
    public const string Burn15 = "burn15";

    /// <summary>Cloques : bulles de chaleur sur les bords.</summary>
    public const string Burn16 = "burn16";

    /// <summary>Ligne de feu : deux fronts qui se croisent en bas.</summary>
    public const string Burn17 = "burn17";

    /// <summary>Surchauffe : balayage orange et bord chaud.</summary>
    public const string Burn18 = "burn18";

    /// <summary>Brûlure lente : halo ambré qui respire.</summary>
    public const string Burn19 = "burn19";

    /// <summary>En feu : flammes en bas et bord incandescent.</summary>
    public const string Burn20 = "burn20";

    /// <summary>Décomposé : monde sépia délavé, bords sombres.</summary>
    public const string Zombie1 = "zombie1";

    /// <summary>Rage : rouge sang qui scintille, bords noirs.</summary>
    public const string Zombie2 = "zombie2";

    /// <summary>Fièvre jaune : monde jauni, bords bruns.</summary>
    public const string Zombie3 = "zombie3";

    /// <summary>Ralenti dramatique, à terre : bandes noires, monde assombri, battement lent.</summary>
    public const string Knock1 = "knock1";

    /// <summary>Teinte violette et pulsation nauséeuse.</summary>
    public const string Poison = "poison";

    /// <summary>Flashs blancs et bord bleu qui grésille.</summary>
    public const string Electrocuted = "electrocuted";

    /// <summary>Aveuglé : écran blanc qui revient lentement.</summary>
    public const string Flashbang = "flashbang";

    /// <summary>Saignement : coulures et vignette qui bat.</summary>
    public const string Bleeding = "bleeding";

    /// <summary>Étourdi : vignette qui tangue, monde flou.</summary>
    public const string Dizzy = "dizzy";

    /// <summary>Teinte acide et grain qui grésille.</summary>
    public const string Radiation = "radiation";

    /// <summary>Bord doré qui bat vite.</summary>
    public const string Adrenaline = "adrenaline";

    /// <summary>Furtif : monde teinté cyan, balayage discret.</summary>
    public const string Stealth = "stealth";

    /// <summary>Bouclier : nid d'abeille bleu et bord.</summary>
    public const string Shield = "shield";

    /// <summary>Soin : halo vert, particules qui montent.</summary>
    public const string Heal = "heal";

    /// <summary>Ralenti : monde bleu-violet, vignette lente.</summary>
    public const string Slowed = "slowed";

    /// <summary>Accéléré : traînées blanches sur les côtés.</summary>
    public const string Haste = "haste";

    /// <summary>Assommé : blanc qui palpite, une onde au centre.</summary>
    public const string Stunned = "stunned";

    /// <summary>Fumigène : écran voilé gris, volutes.</summary>
    public const string Smoke = "smoke";

    /// <summary>Nausée : teinte verdâtre, vignette qui tangue.</summary>
    public const string Nausea = "nausea";

    /// <summary>Thermique : couleurs décalées vers l'orange et le bleu.</summary>
    public const string Thermal = "thermal";

    /// <summary>Vision nocturne : vert lumineux, grain et lignes.</summary>
    public const string Nightvision = "nightvision";

    /// <summary>Choc sourd : onde blanche et flou.</summary>
    public const string Concussion = "concussion";

    /// <summary>Repéré : bord rouge qui clignote.</summary>
    public const string Spotted = "spotted";

    /// <summary>Protection divine : halo doré, poussière qui monte.</summary>
    public const string Divine = "divine";

    /// <summary>Effet perso 1 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_1.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_1_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom1 = "custom1";

    /// <summary>Effet perso 2 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_2.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_2_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom2 = "custom2";

    /// <summary>Effet perso 3 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_3.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_3_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom3 = "custom3";

    /// <summary>Effet perso 4 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_4.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_4_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom4 = "custom4";

    /// <summary>Effet perso 5 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_5.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_5_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom5 = "custom5";

    /// <summary>Effet perso 6 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_6.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_6_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom6 = "custom6";

    /// <summary>Effet perso 7 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_7.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_7_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom7 = "custom7";

    /// <summary>Effet perso 8 : l'image panorama/images/custom_game/uikit/overlay/custom/custom_8.png (1920 x 1080, transparente autour de ce qu'elle montre) + custom_8_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.</summary>
    public const string Custom8 = "custom8";

    /// <summary>Témoin : écran cyan uni, écrit dans uikit_overlay_fx.css. S'il ne s'affiche pas mais que debug1 oui, cette feuille ne charge pas.</summary>
    public const string Debug2 = "debug2";

    /// <summary>Témoin : écran magenta uni, écrit dans uikit_overlay.css (son jumeau <see cref="Debug2"/> est dans uikit_overlay_fx.css).</summary>
    public const string Debug1 = "debug1";

    /// <summary>Témoin : ov_fx lui-même en jaune, sans ses enfants, dans uikit_overlay.css.</summary>
    public const string Debug3 = "debug3";

    /// <summary>Témoin : la boîte des particules en vert (jamais masquée), dans uikit_overlay.css.</summary>
    public const string Debug4 = "debug4";

    /// <summary>Effet perso <paramref name="n"/> (1 à 8) : <c>custom&lt;n&gt;</c>, l'image <c>uikit/overlay/custom/custom_&lt;n&gt;.png</c> de ton addon.</summary>
    public static string Custom(int n) => $"custom{n}";

    /// <summary>The bundled looks, without the custom slots and the checks.</summary>
    public static readonly IReadOnlyList<string> All = [Frozen1, Frozen2, Frozen3, Frozen4, Frozen5, Frozen6, Frozen7, Frozen8, Frozen9, Frozen10, Frozen11, Frozen12, Frozen13, Frozen14, Frozen15, Frozen16, Frozen17, Frozen18, Frozen19, Frozen20, Burn1, Burn2, Burn3, Burn4, Burn5, Burn6, Burn7, Burn8, Burn9, Burn10, Burn11, Burn12, Burn13, Burn14, Burn15, Burn16, Burn17, Burn18, Burn19, Burn20, Zombie1, Zombie2, Zombie3, Knock1, Poison, Electrocuted, Flashbang, Bleeding, Dizzy, Radiation, Adrenaline, Stealth, Shield, Heal, Slowed, Haste, Stunned, Smoke, Nausea, Thermal, Nightvision, Concussion, Spotted, Divine];
}
