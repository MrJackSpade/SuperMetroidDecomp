using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The two side-by-side tilemap screens the original game wrote pictures into (#1514), as oracles compare the port's placed
/// pictures with the words it wrote: the left screen's cells row by row, then the right screen's.
/// </summary>
internal static class TwoScreenTilemapCells
{
    /// <summary>The cells of the two screens.</summary>
    internal const int Count = 2 * ScreenGeometry.TilemapScreenCellCount;

    /// <summary>The word of the two screens a cell of the layer lies in.</summary>
    internal static int IndexOf(Position16<TilemapCells> cell) =>
        cell.X / ScreenGeometry.TilemapScreenWidthInTiles * ScreenGeometry.TilemapScreenCellCount
        + cell.Y * ScreenGeometry.TilemapScreenWidthInTiles + cell.X % ScreenGeometry.TilemapScreenWidthInTiles;

    /// <summary>The cell of the layer word <paramref name="index"/> of the two screens lies in.</summary>
    internal static Position16<TilemapCells> CellAt(int index) =>
        new(index / ScreenGeometry.TilemapScreenCellCount * ScreenGeometry.TilemapScreenWidthInTiles + index % ScreenGeometry.TilemapScreenWidthInTiles,
            index % ScreenGeometry.TilemapScreenCellCount / ScreenGeometry.TilemapScreenWidthInTiles);

    /// <summary>
    /// The picture of each word of the two screens <paramref name="pictures"/> cover, in the order written, each tile of a
    /// picture as the tile of its frame it shows; null where nothing was written.
    /// </summary>
    internal static BgTilePicture?[] Lay(ImmutableArray<PlacedWrittenPicture> pictures)
    {
        var laid = new BgTilePicture?[Count];
        const int tile = ScreenGeometry.BackgroundTileSizePixels;
        foreach (PlacedWrittenPicture placed in pictures)
        {
            for (int row = 0; row < placed.Picture.Size.Y / tile; row++)
            for (int column = 0; column < placed.Picture.Size.X / tile; column++)
            {
                laid[IndexOf(new Position16<TilemapCells>(placed.Corner.X / tile + column, placed.Corner.Y / tile + row))] = new BgTilePicture(placed.Picture,
                    new Position16<ImagePixels>(column * tile, row * tile));
            }
        }
        return laid;
    }
}
