using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The shades the import pre-colours the map sprite sheet in (#1512): each colouring a map sprite draws in on each map
/// screen, and each frame of the map highlight cycle. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class MapSpriteShades
{
    /// <summary>A map sprite's recolourable parts drawn in <paramref name="colouring"/> on <paramref name="screen"/>.</summary>
    public static PictureShade Drawn(MapSpriteScreen screen, MapSpriteColouring colouring) => new($"MapSprite{screen}{colouring}", 0);

    /// <summary>
    /// A map sprite's recolourable parts drawn in the highlight colours of <paramref name="step"/> of the highlight cycle:
    /// the scroll arrows on both map screens.
    /// </summary>
    public static PictureShade Highlight(MapHighlightStep step) => new("MapHighlight", step.Place);
}
