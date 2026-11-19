using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Comms;

/// <summary>
/// A radio transmission. <see cref="UiContent.Style"/> (the hologram tint) and <see cref="UiContent.Classes"/>
/// go on <c>cm_card</c>; <see cref="UiContent.Texts"/> are written as <c>cm_&lt;key&gt;</c>.
/// </summary>
public sealed record CommsOptions : UiContent
{
    public CommsOptions()
    {
        Style = UiStyle.Info;
    }

    /// <summary>
    /// Which face: applied as <c>face-&lt;name&gt;</c> on <c>cm_face</c>, the class that sets the picture. See
    /// <see cref="CommsFaces"/>. Blank shows <see cref="CommsFaces.Unknown"/>.
    /// </summary>
    public string? Face { get; init; }

    /// <summary>Slot <c>{s:cm_name}</c>, who is speaking. Blank collapses it.</summary>
    public string? Name { get; init; }

    /// <summary>Slot <c>{s:cm_text}</c>, what they say. Wraps on up to three lines in the bundled layout.</summary>
    public required string Text { get; init; }

    /// <summary>Slot <c>{s:cm_kicker}</c>, over the card. Blank collapses it.</summary>
    public string? Kicker { get; init; } = "INCOMING TRANSMISSION";

    /// <summary>Slot <c>{s:cm_footer}</c>, under the text. Blank collapses it.</summary>
    public string? Footer { get; init; }

    /// <summary>Stronger jitter on the face (<c>glitch-hard</c>), for a dramatic line.</summary>
    public bool Glitch { get; init; }

    /// <summary>
    /// Seconds of heavy static (<c>static</c>) before the face settles, when the card opens. Zero skips it.
    /// Not played when the content is swapped on a card already up: that one gets a short burst instead.
    /// </summary>
    public float ConnectSeconds { get; init; } = 0.6f;

    /// <summary>
    /// Sound file played on the player's client (<c>play</c>) when the transmission shows: a <c>.vsnd</c> path.
    /// Null = silent.
    /// </summary>
    public string? Sound { get; init; }

    /// <summary>Seconds on screen. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; } = 6f;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required CommsLayout Layout { get; init; }
}
