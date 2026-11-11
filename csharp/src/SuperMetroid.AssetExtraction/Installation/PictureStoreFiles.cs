using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The installation's picture store (#1635), <c>game/pictures/</c>: every picture the sheets show, each written once, on
/// shelf-packed RGBA pages <c>pictures-NNNN.png</c> numbered from 0000, with a recolour-region map
/// (<c>game/recolour-regions/pictures/pictures-NNNN.regions</c>) for a page holding a picture whose pixels of one colour
/// recolour differently, a blend map (<c>game/blend-maps/pictures/pictures-NNNN.blendmap</c>) for a page holding a picture
/// that blends in its pixels, and a provenance manifest of the pages' and maps' hashes. Each sheet manifest names where the
/// store holds each frame's picture, and the loader assembles the sheet's image from the pages. A player edits a page in
/// place, which changes the picture for every frame of every sheet that shows it.
/// </summary>
public static class PictureStoreFiles
{
    /// <summary>The store's directory under the installation's content directory.</summary>
    public const string DirectoryName = "pictures";

    /// <summary>The provenance manifest's file name, in the store's directory.</summary>
    public const string ProvenanceFileName = "pictures-provenance.json";

    /// <summary>The page file holding page <paramref name="page"/>, counted from zero: <c>pictures-0000.png</c> for the first.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is negative.</exception>
    public static string PageFile(int page) => PageName(page) + ".png";

    /// <summary>The recolour-region map file of page <paramref name="page"/> (<see cref="RecolourRegionMapFile"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is negative.</exception>
    public static string RecolourRegionFile(int page) => PageName(page) + RecolourRegionMapFile.Extension;

    /// <summary>The blend map file of page <paramref name="page"/> (<see cref="BlendMapFile"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is negative.</exception>
    public static string BlendMapFileName(int page) => PageName(page) + BlendMapFile.Extension;

    /// <summary>The directory of the store's recolour-region maps under the content directory <paramref name="content"/>.</summary>
    internal static string RecolourRegionDirectory(string content) =>
        Path.Combine(content, GameInstallationLayout.RecolourRegionDirectoryName, DirectoryName);

    /// <summary>The directory of the store's blend maps under the content directory <paramref name="content"/>.</summary>
    internal static string BlendMapDirectory(string content) =>
        Path.Combine(content, GameInstallationLayout.BlendMapDirectoryName, DirectoryName);

    /// <summary>A page's file name without extension.</summary>
    private static string PageName(int page) =>
        "pictures-" + Ensure.AtLeastZero(page).ToString("D4", CultureInfo.InvariantCulture);
}
