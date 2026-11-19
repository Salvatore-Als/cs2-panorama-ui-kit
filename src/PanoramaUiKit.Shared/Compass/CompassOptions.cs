using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Compass;

/// <summary>
/// The compass, driven one of two ways:
/// <list type="bullet">
/// <item><b>Follow</b> (default): the kit reads the player's view and position every <see cref="FollowInterval"/>
/// and moves the strip and the pins itself. Nothing to call after Show. A dead or spectating player keeps the
/// last heading.</item>
/// <item><b>Pushed</b> (<see cref="Follow"/> false): the strip shows <see cref="Heading"/>; call Show again to move it.</item>
/// </list>
/// <see cref="UiContent.Style"/> and <see cref="UiContent.Classes"/> go on <c>cp_bar</c>; <see cref="UiContent.Texts"/>
/// are written as <c>cp_&lt;key&gt;</c>.
/// </summary>
public sealed record CompassOptions : UiContent
{
    /// <summary>Follow the player's view. False = <see cref="Heading"/>.</summary>
    public bool Follow { get; init; } = true;

    /// <summary>Seconds between two reads of the view while following. Only what changed is sent.</summary>
    public float FollowInterval { get; init; } = 0.05f;

    /// <summary>Pushed heading, 0 = north, clockwise, in degrees. Ignored while following.</summary>
    public float Heading { get; init; }

    /// <summary>
    /// Where the bar sits, <c>at-*</c> on <c>cp_bar</c>. By default under the game's team bar, which holds the
    /// alive players and the round timer at the top centre.
    /// </summary>
    public CompassPlacement Placement { get; init; } = CompassPlacement.UnderHud;

    /// <summary>Only the first layout <c>Pins</c> are drawn. <c>SetPins</c> replaces them without a new Show.</summary>
    public IReadOnlyList<CompassPin> Pins { get; init; } = [];

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required CompassLayout Layout { get; init; }
}
