using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>Selected ending/credits Mode-7, OBJ/BG artwork, compositions and palette animation.</summary>
public static class EndingPresentationIdentity
{
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
