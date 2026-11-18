using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The shades the import pre-colours the map sprite sheet in (#1512): each colouring a map sprite draws in on each map
/// screen, and each frame of the map highlight cycle. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class MapSpriteShades
{
    /// <summary>
    /// Each map screen's name in its drawn shades' installed names (#2511), written out so renaming a screen's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<MapSpriteScreen, string> ScreenKeys =
        new Dictionary<MapSpriteScreen, string>
        {
            [MapSpriteScreen.FileSelect] = "FileSelect",
            [MapSpriteScreen.Pause] = "Pause",
        }.ToFrozenDictionary();

    /// <summary>
    /// Each colouring's name in its drawn shades' installed names (#2511), written out so renaming a colouring's identity
    /// never changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<MapSpriteColouring, string> ColouringKeys =
        new Dictionary<MapSpriteColouring, string>
        {
            [MapSpriteColouring.Labels] = "Labels",
            [MapSpriteColouring.DimLabels] = "DimLabels",
            [MapSpriteColouring.Markers] = "Markers",
            [MapSpriteColouring.DefeatedBossMarker] = "DefeatedBossMarker",
            [MapSpriteColouring.Highlighted] = "Highlighted",
        }.ToFrozenDictionary();

    /// <summary>A map sprite's recolourable parts drawn in <paramref name="colouring"/> on <paramref name="screen"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The screen or the colouring has no installed name.</exception>
    public static PictureShade Drawn(MapSpriteScreen screen, MapSpriteColouring colouring) =>
        new($"MapSprite{Ensure.Listed(ScreenKeys, screen)}{Ensure.Listed(ColouringKeys, colouring)}", 0);

    /// <summary>
    /// A map sprite's recolourable parts drawn in the highlight colours, at frame <paramref name="frame"/> of the
    /// highlight cycle: the scroll arrows on both map screens.
    /// </summary>
    public static PictureShade Highlight(MapHighlightFrame frame) => new("MapHighlight", frame.Frame);
}
