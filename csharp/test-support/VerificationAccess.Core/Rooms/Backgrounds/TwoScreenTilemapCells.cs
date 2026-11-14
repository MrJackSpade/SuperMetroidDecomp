using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The two side-by-side tilemap screens the original game wrote pictures into (#1514, #1601), as oracles compare the
/// port's placed pictures with the words it wrote: the left screen's eight-pixel cells row by row, then the right
/// screen's. The port places pictures at pixels of the layer; this oracle alone knows the screens' cells.
/// </summary>
internal static class TwoScreenTilemapCells
{
    /// <summary>The cells of the two screens.</summary>
    internal const int Count = 2 * TilemapScreenGeometry.CellCount;

    /// <summary>The pixels along either side of one cell.</summary>
    private const int CellPixels = ScreenGeometry.BackgroundCellSizePixels;

    /// <summary>The word of the two screens the layer pixel <paramref name="corner"/>, a cell's top-left corner, lies in.</summary>
    internal static int IndexOf(Position16<PlanePixels> corner)
    {
        int column = corner.X / CellPixels, row = corner.Y / CellPixels;
        return column / TilemapScreenGeometry.WidthInTiles * TilemapScreenGeometry.CellCount
            + row * TilemapScreenGeometry.WidthInTiles + column % TilemapScreenGeometry.WidthInTiles;
    }

    /// <summary>The layer pixel of the top-left corner of word <paramref name="index"/> of the two screens.</summary>
    internal static Position16<PlanePixels> CornerAt(int index) =>
        new((index / TilemapScreenGeometry.CellCount * TilemapScreenGeometry.WidthInTiles + index % TilemapScreenGeometry.WidthInTiles) * CellPixels,
            index % TilemapScreenGeometry.CellCount / TilemapScreenGeometry.WidthInTiles * CellPixels);

    /// <summary>
    /// The picture of each word of the two screens <paramref name="pictures"/> cover, in the order written, each cell of a
    /// picture as the cell of its frame it shows; null where nothing was written.
    /// </summary>
    internal static BgTilePicture?[] Lay(ImmutableArray<PlacedWrittenPicture> pictures)
    {
        var laid = new BgTilePicture?[Count];
        foreach (PlacedWrittenPicture placed in pictures)
        {
            for (int y = 0; y < placed.Size.Y; y += CellPixels)
            for (int x = 0; x < placed.Size.X; x += CellPixels)
                laid[IndexOf(placed.Corner + new Offset16<PlanePixels>(x, y))] = new BgTilePicture(placed.Picture, new Position16<ImagePixels>(x, y));
        }
        return laid;
    }
}
