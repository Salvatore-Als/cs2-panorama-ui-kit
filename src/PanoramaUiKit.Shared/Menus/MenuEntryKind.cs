namespace PanoramaUiKit.Shared.Menus;

/// <summary>What a click on an entry does: class <c>kind-&lt;name&gt;</c> on <c>mn_item_&lt;n&gt;</c>.</summary>
public enum MenuEntryKind
{
    /// <summary>Runs <see cref="MenuEntry.OnClick"/>.</summary>
    Button,

    /// <summary>Flips an on / off switch (class <c>on</c>) and reports the new state.</summary>
    Toggle,

    /// <summary>Steps to the next choice (shown as the value) and reports it.</summary>
    Select,

    /// <summary>Opens <see cref="MenuEntry.Submenu"/> on top, with a back button.</summary>
    Submenu,
}
