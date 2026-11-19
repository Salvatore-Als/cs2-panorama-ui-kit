using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Screamers;

public sealed record ScreamerOptions : UiContent
{
    /// <summary>
    /// Which screamer: applied as <c>scare-&lt;name&gt;</c>, the class that sets the picture. See
    /// <see cref="ScreamerNames"/>. Blank hides the screamer.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether <see cref="Sound"/> is played when the screamer shows. False = picture only.
    ///
    /// <para>The sound is played by the server on the player's client (<c>play</c>), not by the stylesheet: a CSS
    /// <c>sound</c> is loaded on use and reaches the player at an unknown time. It starts
    /// <see cref="SoundLeadSeconds"/> after the picture.</para>
    /// </summary>
    public bool PlaySound { get; init; } = true;

    /// <summary>
    /// Sound file played on the player's client (<c>play</c>) when the screamer shows: a <c>.vsnd</c> path from
    /// the addon. Blank = silent. Not played again when the same screamer is shown while up.
    /// </summary>
    public string? Sound { get; init; } = ScreamerNames.Sound;

    /// <summary>
    /// Seconds between the picture and the sound: the picture shows at once, the sound starts this long after.
    /// Only when a sound is played; zero plays both at once.
    /// </summary>
    public float SoundLeadSeconds { get; init; } = 1f;

    /// <summary>Seconds on screen. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; } = 0.6f;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required ScreamerLayout Layout { get; init; }
}
