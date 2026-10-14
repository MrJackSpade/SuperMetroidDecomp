using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="SamusSpritemapArtworkCatalog"/> members production does not use.</summary>
internal static class SamusSpritemapArtworkCatalogAccess
{
    /// <summary>Inspects installed Samus compositions by their private native-pointer index.</summary>
    extension(SamusSpritemapArtworkCatalog self)
    {
        /// <summary>Enumerates the stored spritemap definitions without recreating stock art.</summary>
        internal IReadOnlyCollection<SamusSpritemapDefinition> Definitions => self.definitions.Values;
    }
}
