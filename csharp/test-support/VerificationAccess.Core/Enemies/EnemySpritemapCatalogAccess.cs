namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="EnemySpritemapCatalog"/> members production does not use.</summary>
internal static class EnemySpritemapCatalogAccess
{
    /// <summary>Exposes <see cref="EnemySpritemapCatalog"/> state that only verification reads.</summary>
    extension(EnemySpritemapCatalog self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => EnemySpritemapsConstants.ContentIdentity(self);
    }
}
