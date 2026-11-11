using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="MapSpriteCompositions"/> members production does not use.</summary>
internal static class MapSpriteCatalogAccess
{
    /// <summary>Loads the installed map-object characters.</summary>
    extension(MapSpriteCompositions self)
    {
        /// <summary>Loads the installed shared 4-bpp map-object characters into the screen's OBJ character region.</summary>
        public void LoadArtworkTo(VramRegion destination) => destination.Load(MapSpriteArtwork.Of(self).Page.Characters);
    }
}
