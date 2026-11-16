using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Selected ending scene backdrops. The ending's object maps and compositions are import data (#1638) and the turning floor's
/// identity is its composed sheets' (#1555): the sprite sheets' identity covers the pictures composed from both.
/// </summary>
public static class EndingPresentationIdentity
{
    /// <summary>Captures named identities of the selected ending object and backdrop content for installation/replay compatibility metadata.</summary>
    /// <param name="backdrops">Non-null selected ending scene backdrops (#1544).</param>
    /// <returns>A new ordinal-keyed dictionary mapping the backdrops' installation domain name to their decoded-content identity string.</returns>
    /// <remarks>Uses the supplied catalogs; does not load files, retain catalog references, or conflate raw-file hashes with engine-build/cartridge provenance.</remarks>
    public static IReadOnlyDictionary<string, string> Create(EndingBackdrops backdrops) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.EndingBackdropDirectoryName] = backdrops.ContentIdentity,
        };
}
