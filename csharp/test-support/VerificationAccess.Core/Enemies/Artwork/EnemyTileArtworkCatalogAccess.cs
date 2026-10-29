using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="EnemyTileArtworkCatalog"/> members production does not use.</summary>
internal static class EnemyTileArtworkCatalogAccess
{
    /// <summary>Constructs a partial enemy-art catalog for focused verification.</summary>
    extension(EnemyTileArtworkCatalog)
    {
        /// <summary>Explicitly partial constructed artwork; never an installed production catalog.</summary>
        internal static EnemyTileSourceArtwork FromArtworkForVerification(EnemyTileArtworkParts parts) =>
            new EnemyTileSourceArtwork(Ensure.NotNull(parts));
    }
}
