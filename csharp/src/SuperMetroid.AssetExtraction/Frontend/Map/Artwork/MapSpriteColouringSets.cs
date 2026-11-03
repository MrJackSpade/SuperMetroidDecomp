using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>
/// Which of a map screen's sprite sets each map sprite colouring is (#1512): the cartridge draws each colouring's
/// palette-inheriting parts in that set, and the import pre-colours the colouring's shade from it.
/// </summary>
internal static class MapSpriteColouringSets
{
    /// <summary>The sprite set each colouring draws in.</summary>
    private static readonly Dictionary<MapSpriteColouring, int> Sets = new()
    {
        [MapSpriteColouring.Labels] = 0,
        [MapSpriteColouring.DimLabels] = 1,
        [MapSpriteColouring.DefeatedBossMarker] = 6,
        [MapSpriteColouring.Markers] = 7,
    };

    /// <summary>The sprite set <paramref name="colouring"/> draws in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is not a map sprite colouring.</exception>
    internal static int SpriteSet(MapSpriteColouring colouring) => Sets.TryGetValue(colouring, out int set)
        ? set
        : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "Not a map sprite colouring.");
}
