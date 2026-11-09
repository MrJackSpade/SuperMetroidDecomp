using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="EnemyTileSourceArtwork"/> members production does not use.</summary>
internal static class EnemyTileSourceArtworkAccess
{
    /// <summary>Constructs a partial enemy-art catalog for focused verification.</summary>
    extension(EnemyTileSourceArtwork)
    {
        /// <summary>Explicitly partial constructed artwork; never an installed production catalog.</summary>
        internal static EnemyTileSourceArtwork FromArtworkForVerification(EnemyTileArtworkParts parts) =>
            new EnemyTileSourceArtwork(Ensure.NotNull(parts));
    }
}
