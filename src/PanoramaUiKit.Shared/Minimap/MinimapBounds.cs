namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// The world rectangle the map picture shows, in game units: <see cref="MinX"/> is its left edge,
/// <see cref="MaxY"/> its top edge (+Y is up on the picture, as on the game's radar).
/// </summary>
public sealed record MinimapBounds(float MinX, float MinY, float MaxX, float MaxY)
{
    /// <summary>
    /// From a radar overview file (<c>resource/overviews/&lt;map&gt;.txt</c> in the game: <c>pos_x</c>,
    /// <c>pos_y</c>, <c>scale</c>), whose picture is <paramref name="size"/> pixels square. <c>pos_x</c> / <c>pos_y</c> is
    /// the world point at the top-left corner, <c>scale</c> the units per pixel.
    /// </summary>
    public static MinimapBounds FromOverview(float posX, float posY, float scale, float size = 1024f) =>
        new(posX, posY - scale * size, posX + scale * size, posY);

    public float Width => MaxX - MinX;

    public float Height => MaxY - MinY;

    /// <summary>A world point as a share of the picture, 0..1 from the left and from the top. Not clamped.</summary>
    public (float U, float V) Normalize(float x, float y) =>
        (Width == 0f ? 0f : (x - MinX) / Width, Height == 0f ? 0f : (MaxY - y) / Height);
}
