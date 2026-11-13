using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The installation's picture store (#1635, #1644), <c>game/pictures/</c>: every picture the sheets show, each written once
/// by its pixels, on shelf-packed RGBA pages <c>pictures-NNNN.png</c> numbered from 0000, and its index, which lists the
/// pages, every picture's place and every map's tag (#1693). A picture's pixels are all it is: the recolour regions and blending it is shown with are maps of
/// the frame that shows it, each map stored once in its own folder, the recolour-region maps in
/// <c>game/recolour-regions/pictures.regions</c> and the blend maps in <c>game/blend-maps/pictures.blendmap</c>. Each sheet
/// manifest names where the store holds each frame's picture and the maps the frame shows it with, and the loader assembles
/// the sheet's image from the pages and the maps. A player edits a page in place, which changes the picture for every frame
/// of every sheet that shows it.
/// </summary>
public static class PictureStoreFiles
{
    /// <summary>The store's directory under the installation's content directory.</summary>
    public const string DirectoryName = "pictures";

    /// <summary>The index's file name, in the store's directory.</summary>
    public const string IndexFileName = "pictures.json";

    /// <summary>The page file holding page <paramref name="page"/>, counted from zero: <c>pictures-0000.png</c> for the first.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is negative.</exception>
    public static string PageFile(int page) => PageName(page) + ".png";

    /// <summary>
    /// The file of every recolour-region map the store's pictures are shown with (<see cref="RecolourRegionMapFile"/>), under
    /// the content directory <paramref name="content"/>.
    /// </summary>
    internal static string RecolourRegionMapsFile(string content) =>
        Path.Combine(content, GameInstallationLayout.RecolourRegionDirectoryName, DirectoryName + RecolourRegionMapFile.Extension);

    /// <summary>
    /// The file of every blend map the store's pictures are shown with (<see cref="BlendMapFile"/>), under the content
    /// directory <paramref name="content"/>.
    /// </summary>
    internal static string BlendMapsFile(string content) =>
        Path.Combine(content, GameInstallationLayout.BlendMapDirectoryName, DirectoryName + BlendMapFile.Extension);

    /// <summary>A page's file name without extension.</summary>
    private static string PageName(int page) =>
        "pictures-" + Ensure.AtLeastZero(page).ToString("D4", CultureInfo.InvariantCulture);
}
