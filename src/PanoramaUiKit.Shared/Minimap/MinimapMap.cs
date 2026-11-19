namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// What a map is drawn with: the picture (a <see cref="MinimapMaps"/> name, usually <see cref="MinimapMaps.Slot"/> for a
/// custom map) and the world rectangle it covers.
/// </summary>
/// <param name="Picture"><c>map-&lt;picture&gt;</c> on <c>mm_map</c>: a picture the stylesheet has a rule for.</param>
/// <param name="Bounds">The world rectangle the picture covers, edge to edge.</param>
public sealed record MinimapMap(string Picture, MinimapBounds Bounds);
