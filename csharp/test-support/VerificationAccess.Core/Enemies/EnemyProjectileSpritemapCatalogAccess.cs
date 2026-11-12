using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="EnemyProjectileSpritemapCatalog"/> members production does not use.</summary>
internal static class EnemyProjectileSpritemapCatalogAccess
{
    /// <summary>Exposes <see cref="EnemyProjectileSpritemapCatalog"/> state that only verification reads.</summary>
    extension(EnemyProjectileSpritemapCatalog self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => EnemyProjectileSpritemapsConstants.ContentIdentity(self);
    }
}
