namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="EnemyExtendedFrameCatalog"/> members production does not use.</summary>
internal static class EnemyExtendedFrameCatalogAccess
{
    /// <summary>Exposes <see cref="EnemyExtendedFrameCatalog"/> state that only verification reads.</summary>
    extension(EnemyExtendedFrameCatalog self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => EnemyExtendedFramesConstants.ContentIdentity(self);
    }
}
