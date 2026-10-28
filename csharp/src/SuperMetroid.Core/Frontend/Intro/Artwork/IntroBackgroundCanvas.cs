using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Writes one illustrated intro background page into its slice of the combined pages.</summary>
/// <param name="output">The combined pages, in native page order.</param>
/// <param name="page">The page this canvas draws.</param>
internal sealed class IntroBackgroundCanvas(BackgroundTile[] output, int page)
{
    /// <summary>Each illustrated page has 32 rows and columns of cells.</summary>
    private static readonly Extent16<TilemapCells> PageSize = new(32, 32);

    /// <summary>Cells in one page.</summary>
    internal const int Cells = 32 * 32;

    /// <summary>Places a tile at the page-local cell.</summary>
    internal void Put(Position16<TilemapCells> cell, BackgroundTile tile) => output[page * Cells + PageSize.RowMajorIndex(cell)] = tile;
    /// <summary>Paints a rectangular run with one tile, used for backgrounds and blank regions.</summary>
    internal void Fill(Position16<TilemapCells> origin, Extent16<TilemapCells> size, BackgroundTile tile)
    {
        for (int row = 0; row < size.Y; row++)
        for (int column = 0; column < size.X; column++) Put(origin + new Offset16<TilemapCells>(checked((short)column), checked((short)row)), tile);
    }
    /// <summary>Copies a row-major atlas patch, mirroring each tile as well when reflected.</summary>
    internal void Patch(Position16<TilemapCells> origin, Extent16<TilemapCells> size, BackgroundTile first, TileFlip flips = TileFlip.None)
    {
        for (int row = 0; row < size.Y; row++)
        for (int column = 0; column < size.X; column++)
        {
            int sx = (flips & TileFlip.Horizontal) != TileFlip.None ? size.X - 1 - column : column;
            int sy = (flips & TileFlip.Vertical) != TileFlip.None ? size.Y - 1 - row : row;
            Put(origin + new Offset16<TilemapCells>(checked((short)column), checked((short)row)),
                (first with { Tile = first.Tile + sy * IntroBackgroundPageDefinitions.AtlasColumns + sx }).WithFlipsToggled(flips));
        }
    }
}
