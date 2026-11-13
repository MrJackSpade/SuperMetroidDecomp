using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// One sheet family's converted sheets (#1635), as its composer yields them to the picture store, which writes their
/// pictures once for the whole installation and then their manifests into the family's directory.
/// </summary>
/// <param name="Directory">The family's directory, directly under the content directory the store is written to.</param>
/// <param name="Sheets">The family's sheets, each under its installed name.</param>
internal sealed record InstalledSheetFamily(string Directory, IReadOnlyList<NamedInstalledSheet> Sheets)
{
    /// <summary>
    /// The family of <paramref name="sheets"/>, each converted as the installation holds it (#1633); the sheets convert in
    /// parallel, each into its own place, so the family's order stays the sheets' own.
    /// </summary>
    /// <param name="directory">The family's directory.</param>
    /// <param name="sheets">The composed sheets, each under its installed name.</param>
    internal static InstalledSheetFamily Install(string directory, IReadOnlyList<NamedComposedSheet> sheets)
    {
        Ensure.NotNull(sheets);
        var installed = new NamedInstalledSheet[sheets.Count];
        Parallel.For(0, sheets.Count, index =>
        {
            ComposedSpriteSheet sheet = Ensure.NotNull(sheets[index].Sheet);
            installed[index] = new NamedInstalledSheet(sheets[index].Name,
                InstalledSheetConversion.Install(sheet.Manifest, sheet.Size, sheet.Pixels, sheet.PaletteIndices) with { ShownWhole = sheet.ShownWhole });
        });
        return new InstalledSheetFamily(directory, installed);
    }

    /// <summary>The family's directory name, which orders it among the families the store writes.</summary>
    internal string DirectoryName => Path.GetFileName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Directory)));
}
