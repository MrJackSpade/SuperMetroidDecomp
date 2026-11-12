using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The tilemap geometry the import pictures a library room background from (#1502): the two BG2 pages it reads, the one
/// period of them it installs (#1651), and the tiles of one changing cell. The game only shows the installed picture.
/// </summary>
internal static class RoomBackgroundImageGeometry
{
    /// <summary>The tilemap pages a library background's list leaves on BG2.</summary>
    internal const int Pages = 2;

    /// <summary>Tiles along a tilemap page's side.</summary>
    internal const int PageTiles = 32;

    /// <summary>Pixels along a tilemap page's side: the width of every background picture, one page wide.</summary>
    internal const int PagePixels = PageTiles * 8;

    /// <summary>Tiles in one tilemap page.</summary>
    internal const int PageWords = PageTiles * PageTiles;

    /// <summary>Tiles in a library background's two pages.</summary>
    internal const int Words = Pages * PageWords;

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

    /// <summary>
    /// One period of what a library background shows (#1651), from the two BG2 pages <paramref name="pages"/> its list
    /// leaves: where the second page holds the same tilemap, the first page's rows down to their shortest period of whole
    /// cells, the first page whole where its rows do not repeat; otherwise both pages, the first above the second.
    /// </summary>
    /// <remarks>
    /// The two pages are one layer that repeats beyond its edges: gameplay sets BG2's map to $4800 with two 32x32 pages side
    /// by side (<c>BG2SC</c> <c>$49</c>, written at <c>$82:821F</c>, <c>$82:A337</c> and <c>$88:83D6</c>), and the Tourian
    /// entrance's statues set the same two pages one above the other (<c>$4A</c>, <c>$88:DBA4</c>); either way the hardware
    /// wraps the scroll past the map's edges, so the layer shows the pages over and over, across and down. Where the second
    /// page holds the first page's tilemap word for word, as every list that copies one staged page to both pages leaves
    /// them (a 2 KiB transfer from the staging to each page, such as <c>Crateria0Rocks</c>'s), every tile of the layer shows
    /// the first page's tile at the same place in its page, over the same characters and colours: the layer is the one page
    /// repeated across and down with the page's period. A background whose pages differ, such as Kraid's or the Tourian
    /// statues', keeps both.
    /// </remarks>
    /// <exception cref="ArgumentException">The words are not two pages' tiles.</exception>
    internal static ushort[] OnePeriod(ushort[] pages)
    {
        Ensure.LengthEqual(Ensure.NotNull(pages), Words);
        if (!pages.AsSpan(0, PageWords).SequenceEqual(pages.AsSpan(PageWords)))
            return pages;
        // The one page repeats down too: where its rows repeat with a shorter period of whole cells, the layer is those rows
        // repeated, so they are the period (a tower's or a wall's repeating pattern).
        for (int rows = CellSideTiles; rows < PageTiles; rows += CellSideTiles)
        {
            if (PageTiles % rows == 0 && RepeatsDown(pages, rows))
                return pages[..(rows * PageTiles)];
        }
        return pages[..PageWords];
    }

    /// <summary>Tiles along a 16x16 cell's side: a period down is a whole number of cells, as the changing cells need.</summary>
    private const int CellSideTiles = 2;

    /// <summary>Whether every row of the first page of <paramref name="pages"/> equals the row <paramref name="rows"/> rows above it.</summary>
    private static bool RepeatsDown(ushort[] pages, int rows)
    {
        for (int row = rows; row < PageTiles; row++)
        {
            if (!pages.AsSpan(row * PageTiles, PageTiles).SequenceEqual(pages.AsSpan((row - rows) * PageTiles, PageTiles)))
                return false;
        }
        return true;
    }
}
