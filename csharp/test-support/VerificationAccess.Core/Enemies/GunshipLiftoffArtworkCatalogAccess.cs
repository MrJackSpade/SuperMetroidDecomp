using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="GunshipLiftoffArtworkCatalog"/> members production does not use.</summary>
internal static class GunshipLiftoffArtworkCatalogAccess
{
    /// <summary>Exposes <see cref="GunshipLiftoffArtworkCatalog"/> state that only verification reads.</summary>
    extension(GunshipLiftoffArtworkCatalog self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => GunshipLiftoffArtworkConstants.ContentIdentity(self.frames);
    }
}
