using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="MapSpriteCatalog"/> members production does not use.</summary>
internal static class MapSpriteCatalogAccess
{
    /// <summary>Accesses a compiled world-map label through the catalog's private selector.</summary>
    private static WorldMapLabelComposition? WorldLabel(MapSpriteCatalog catalog, MapSpriteId id) =>
        catalog.GetWorldLabel(id);

    /// <summary>Inspects authored compositions and sparse map-object pixel storage.</summary>
    extension(MapSpriteCatalog self)
    {
        /// <summary>Checks whether a world label has authored parts or a non-label has an installed frame.</summary>
        internal bool StoresComposition(MapSpriteId id) => WorldLabel(self, id) is { } label
            ? label.authored is not null
            : self.GetFrame(id) is not null;

        /// <summary>Stored horizontal advances: the label origin(s) plus each authored letter advance.</summary>
        internal int StoredLabelHorizontalCount(MapSpriteId id)
        {
            if (WorldLabel(self, id) is not { } label || label.authored is not null)
                return 0;
            int origins = label.identity == MapSpriteId.WorldWreckedShip ? 2 : 1;
            return origins + (label.letterAdvances?.Count ?? 0);
        }

        /// <summary>Counts sparse highlight pixels in the map-object character art.</summary>
        internal int StoredHighlightPixelCount => MapSpriteArtwork.Of(self).characters.StoredHighlightPixelCount;

        /// <summary>Counts sparse reserve pixels in the map-object character art.</summary>
        internal int StoredReservePixelCount => MapSpriteArtwork.Of(self).characters.StoredReservePixelCount;

        /// <summary>Measures the catalog's independent map-object character byte payload.</summary>
        internal int StoredArtworkByteCount => MapSpriteArtwork.Of(self).characters.StoredOtherByteCount;
        /// <summary>Loads the compiled shared 4-bpp map-object characters into the screen's OBJ character region.</summary>
        public void LoadArtworkTo(VramRegion destination) => MapSpriteArtwork.Of(self).characters.LoadTo(destination);
    }
}
