using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// One picture a frontend screen shows (#1467), cut from the page the cartridge would lay it out on: its frame name, the
/// page cell its top-left cell lies on, and its tiles row by row, none where the picture paints nothing. The composed
/// frame's origin is the page's corner, so the game places every picture of a page at the page's own corner.
/// </summary>
/// <param name="Name">The picture's frame name.</param>
/// <param name="Offset">The cells from the point the game places the picture at (a page's corner, or a field's anchor) to its top-left cell.</param>
/// <param name="Size">The picture's width and height in cells.</param>
/// <param name="Tiles">Its tiles row by row; none where it paints nothing.</param>
internal sealed record ScreenPictureSubject(string Name, Offset16<TilemapCells> Offset, Extent16<TilemapCells> Size, BackgroundTile?[] Tiles)
{
    /// <summary>The whole of <paramref name="page"/>, <paramref name="width"/> cells across, except <paramref name="omitted"/> cells.</summary>
    /// <exception cref="ArgumentException">The page is not whole rows.</exception>
    internal static ScreenPictureSubject Page(string name, ReadOnlySpan<BackgroundTile> page, int width, IEnumerable<int>? omitted = null)
    {
        Ensure.BetweenInclusive(1, int.MaxValue, width);
        if (page.IsEmpty || page.Length % width != 0)
            throw new ArgumentException($"A {page.Length}-cell page is not whole {width}-cell rows.", nameof(page));
        var tiles = new BackgroundTile?[page.Length];
        for (int cell = 0; cell < page.Length; cell++)
            tiles[cell] = page[cell];
        foreach (int cell in omitted ?? [])
            tiles[cell] = null;
        return new(name, default, new(checked((ushort)width), checked((ushort)(page.Length / width))), tiles);
    }

    /// <summary>
    /// The picture of <paramref name="cells"/> of <paramref name="page"/>, <paramref name="width"/> cells across: the cells'
    /// bounding box, painting only the cells named.
    /// </summary>
    /// <exception cref="ArgumentException">No cell is named, or a cell lies outside the page.</exception>
    internal static ScreenPictureSubject Cells(string name, ReadOnlySpan<BackgroundTile> page, int width, IReadOnlyCollection<int> cells)
    {
        Ensure.BetweenInclusive(1, int.MaxValue, width);
        Ensure.NotNull(cells);
        if (cells.Count == 0)
            throw new ArgumentException($"Picture {name} names no cells.", nameof(cells));
        foreach (int cell in cells)
        {
            if ((uint)cell >= (uint)page.Length)
                throw new ArgumentException($"Picture {name} names cell {cell}, outside the {page.Length}-cell page.", nameof(cells));
        }
        int left = cells.Min(cell => cell % width), right = cells.Max(cell => cell % width);
        int top = cells.Min(cell => cell / width), bottom = cells.Max(cell => cell / width);
        int across = right - left + 1;
        var tiles = new BackgroundTile?[across * (bottom - top + 1)];
        foreach (int cell in cells)
            tiles[(cell / width - top) * across + cell % width - left] = page[cell];
        return new(name, new(checked((short)left), checked((short)top)), new(checked((ushort)across), checked((ushort)(bottom - top + 1))), tiles);
    }

    /// <summary>A <paramref name="size"/> block of <paramref name="tiles"/>, row by row, whose top-left cell lies <paramref name="offset"/> from where the game places it.</summary>
    /// <exception cref="ArgumentException">The tiles do not fill the block.</exception>
    internal static ScreenPictureSubject Block(string name, Offset16<TilemapCells> offset, Extent16<TilemapCells> size, ReadOnlySpan<BackgroundTile> tiles)
    {
        Ensure.LengthEqual(tiles, size.X * size.Y);
        var painted = new BackgroundTile?[tiles.Length];
        for (int index = 0; index < tiles.Length; index++)
            painted[index] = tiles[index];
        return new(name, offset, size, painted);
    }
}
