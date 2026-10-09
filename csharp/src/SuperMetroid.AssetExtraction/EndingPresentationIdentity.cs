using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>Selected ending/credits Mode-7, OBJ/BG artwork, compositions and palette animation.</summary>
public static class EndingPresentationIdentity
{
    /// <summary>Captures named identities of the selected ending Mode-7, object, and palette content for installation/replay compatibility metadata.</summary>
    /// <param name="mode7">Non-null selected ending Mode-7 artwork catalog.</param>
    /// <param name="objects">Non-null selected ending OBJ/BG character and composition catalog.</param>
    /// <param name="palettes">Non-null selected ending palette/animation catalog.</param>
    /// <returns>A new ordinal-keyed dictionary mapping the three installation domain names to their decoded-content identity strings.</returns>
    /// <remarks>Uses the supplied catalogs, including any already selected overrides; does not load files, retain catalog references, or conflate raw-file hashes with engine-build/cartridge provenance.</remarks>
    public static IReadOnlyDictionary<string, string> Create(
        EndingMode7ArtworkCatalog mode7, EndingObjectArtworkCatalog objects, EndingPaletteCatalog palettes) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.EndingMode7DirectoryName] = mode7.ContentIdentity,
            [GameInstallationLayout.EndingObjectDirectoryName] = objects.ContentIdentity,
            [GameInstallationLayout.EndingPaletteDirectoryName] = palettes.ContentIdentity,
        };

    /// <summary>Loads the same selected domains for installed diagnostic-artifact import.</summary>
    public static IReadOnlyDictionary<string, string> Load(GameInstallation installation)
    {
        ArgumentNullException.ThrowIfNull(installation);
        return Create(installation.LoadEndingMode7Art(), installation.LoadEndingObjectArt(),
            installation.LoadEndingPalettes());
    }
}
