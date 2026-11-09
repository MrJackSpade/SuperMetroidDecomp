using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Selected ending/credits OBJ/BG artwork, compositions and scene backdrops. The turning floor's identity is its composed
/// sheets', which the sprite sheets' identity covers (#1555).
/// </summary>
public static class EndingPresentationIdentity
{
    /// <summary>Captures named identities of the selected ending object and backdrop content for installation/replay compatibility metadata.</summary>
    /// <param name="objects">Non-null selected ending OBJ/BG character and composition catalog.</param>
    /// <param name="backdrops">Non-null selected ending scene backdrops (#1544).</param>
    /// <returns>A new ordinal-keyed dictionary mapping the two installation domain names to their decoded-content identity strings.</returns>
    /// <remarks>Uses the supplied catalogs; does not load files, retain catalog references, or conflate raw-file hashes with engine-build/cartridge provenance.</remarks>
    public static IReadOnlyDictionary<string, string> Create(EndingObjectArtwork objects, EndingBackdrops backdrops) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.EndingObjectDirectoryName] = objects.ContentIdentity,
            [GameInstallationLayout.EndingBackdropDirectoryName] = backdrops.ContentIdentity,
        };
}
