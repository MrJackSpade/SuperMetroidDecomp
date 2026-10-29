using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// Verification access to the installed boss artwork's import-side frames (#1466): the runtime binds the installation's
/// <see cref="BossSourceArtwork"/>, whose body frames gameplay no longer reads.
/// </summary>
internal static class BossArtworkCatalogAccess
{
    /// <summary>The import's view of installed boss artwork.</summary>
    extension(BossArtworkCatalog catalog)
    {
        /// <summary>The installed artwork as the import holds it, with the frames the body sheets are composed from.</summary>
        /// <exception cref="InvalidOperationException">The artwork was not loaded from an installation.</exception>
        internal BossSourceArtwork Sources => catalog as BossSourceArtwork
            ?? throw new InvalidOperationException($"Boss artwork {catalog.GetType().Name} is not installed source artwork.");
    }
}
