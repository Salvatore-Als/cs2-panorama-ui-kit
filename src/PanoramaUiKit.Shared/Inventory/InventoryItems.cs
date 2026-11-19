namespace PanoramaUiKit.Shared.Inventory;

/// <summary>
/// The items the bundled stylesheet has an icon for. Each name is a class <c>item-&lt;name&gt;</c> on
/// <c>inv_&lt;n&gt;_icon</c>, whose background is <c>panorama/images/custom_game/uikit/items/&lt;name&gt;.vsvg</c>.
/// A custom layout can add its own names.
/// </summary>
public static class InventoryItems
{
    public const string FlashlightOn = "flashlight_on";
    public const string FlashlightOff = "flashlight_off";
    public const string Ammo = "ammo";
    public const string Medkit = "medkit";
    public const string Bandage = "bandage";
    public const string Armor = "armor";
    public const string Grenade = "grenade";
    public const string Battery = "battery";
    public const string Key = "key";
    public const string Food = "food";

    public static readonly IReadOnlyList<string> All =
        [FlashlightOn, FlashlightOff, Ammo, Medkit, Bandage, Armor, Grenade, Battery, Key, Food];

    /// <summary>Class of an item on <c>inv_&lt;n&gt;_icon</c>: <c>item-&lt;name&gt;</c>.</summary>
    public static string Class(string name) => $"item-{name}";
}
