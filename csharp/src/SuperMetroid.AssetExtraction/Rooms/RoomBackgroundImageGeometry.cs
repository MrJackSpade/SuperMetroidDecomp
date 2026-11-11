using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The tilemap geometry the import pictures a library room background from (#1502): the two BG2 pages it reads and the
/// tiles of one changing cell. The game only shows the installed picture.
/// </summary>
internal static class RoomBackgroundImageGeometry
{
    /// <summary>The tilemap pages a library background pictures.</summary>
    internal const int Pages = 2;

    /// <summary>Tiles along a tilemap page's side.</summary>
    internal const int PageTiles = 32;

    /// <summary>Pixels along a tilemap page's side: the width of every background picture, one page wide.</summary>
    internal const int PagePixels = PageTiles * 8;

    /// <summary>Tiles in a library background's two pages.</summary>
    internal const int Words = Pages * PageTiles * PageTiles;

    /// <summary>Tiles in one 16x16 cell of an image.</summary>
    internal const int CellTiles = 4;

    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = 8;

    /// <summary>
    /// The width and height of the image that pictures <paramref name="words"/> tiles, one page wide and their rows down:
    /// the size the installed image records apart from its picture's parts, either of which may show nothing (#1634).
    /// </summary>
    /// <exception cref="InvalidDataException">The tiles do not fill whole rows of a page.</exception>
    internal static Extent16<ImagePixels> ImageSize(int words)
    {
        if (words <= 0 || words % PageTiles != 0)
            throw new InvalidDataException($"A room background of {words} tiles does not fill whole rows of a {PageTiles}-tile page.");
        return new Extent16<ImagePixels>(PagePixels, checked((ushort)(words / PageTiles * TilePixels)));
    }
}
