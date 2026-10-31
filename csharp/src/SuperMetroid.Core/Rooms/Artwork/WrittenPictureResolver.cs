using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what pictures written over a room's background show (#1392, #1466): each written picture's frame in the room's
/// written picture sheet for the values the appearance inputs it depends on take now, laid over the tilemap cells it covers
/// from the cell it was written at, a later picture over an earlier one.
/// </summary>
public sealed class WrittenPictureResolver(WrittenPictureSheet sheet)
{
    /// <summary>Tilemap cells in a row of a background's screen.</summary>
    private const int RowCells = ScreenGeometry.TilemapScreenWidthInTiles;

    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = ScreenGeometry.BackgroundTileSizePixels;

    /// <summary>What was last resolved: an overlay or a BG3 plane.</summary>
    private object? source;

    /// <summary>The frame each written picture showed when last resolved.</summary>
    private SpriteSheetFrame[] shown = [];

    /// <summary>Each cell's picture as last resolved, null where nothing is written.</summary>
    private ImmutableArray<BgTilePicture?> cells;

    /// <summary>The sheet.</summary>
    public WrittenPictureSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>
    /// Each tilemap cell's picture of what is written in <paramref name="overlay"/> while the room's inputs take the values
    /// <paramref name="appearance"/> reads; null where nothing is written.
    /// </summary>
    /// <exception cref="InvalidOperationException">The sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures a written picture for no such values, or it runs off the tilemap.</exception>
    public ImmutableArray<BgTilePicture?> Resolve(RoomWrittenOverlay overlay, RoomAppearanceView appearance)
    {
        if (Sheet.Layer != WrittenPictureLayer.Bg12)
            throw new InvalidOperationException($"{Sheet.Name} pictures nothing written over BG1 or BG2.");
        return ResolvePlaced(Ensure.NotNull(overlay), RoomWrittenOverlay.CellCount, null, overlay.Pictures, appearance);
    }

    /// <summary>
    /// Each of <paramref name="cellCount"/> cells' picture: <paramref name="fill"/>'s one-tile picture, or nothing when it is
    /// null, with <paramref name="placed"/> laid over the cells in order, while the room's inputs take the values
    /// <paramref name="appearance"/> reads; re-laid only when <paramref name="placement"/> is another than last resolved or a
    /// picture shows another frame. The layer that owns the placement gives it, as the BG3 plane's do.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The sheet pictures a written picture for no such values, the fill is not one tile, or a picture runs off the cells.
    /// </exception>
    internal ImmutableArray<BgTilePicture?> ResolvePlaced(object placement, int cellCount, string? fill, IReadOnlyList<WrittenPicture> placed,
        RoomAppearanceView appearance)
    {
        Ensure.NotNull(placed);
        int filled = fill is null ? 0 : 1;
        var frames = new SpriteSheetFrame[filled + placed.Count];
        if (fill is not null)
            frames[0] = Sheet.Picture(fill, appearance);
        for (int index = 0; index < placed.Count; index++)
            frames[filled + index] = Sheet.Picture(placed[index].Picture, appearance);
        bool unchanged = ReferenceEquals(placement, source) && shown.Length == frames.Length;
        for (int index = 0; unchanged && index < frames.Length; index++)
            unchanged = ReferenceEquals(frames[index], shown[index]);
        if (unchanged)
            return cells;
        var next = new BgTilePicture?[cellCount];
        if (fill is not null)
        {
            if (frames[0].Size.X != TilePixels || frames[0].Size.Y != TilePixels)
                throw new InvalidDataException($"{Sheet.Name} pictures {fill} as more than one tile.");
            Array.Fill(next, new BgTilePicture(frames[0], default));
        }
        for (int index = 0; index < placed.Count; index++)
            Lay(next, placed[index], frames[filled + index]);
        source = placement;
        shown = frames;
        cells = [.. next];
        return cells;
    }

    /// <summary>Lays each tile of <paramref name="frame"/> over the cells <paramref name="written"/> covers from its cell.</summary>
    /// <exception cref="InvalidDataException">The picture runs past the end of a row or of the cells.</exception>
    private void Lay(BgTilePicture?[] laid, WrittenPicture written, SpriteSheetFrame frame)
    {
        int columns = frame.Size.X / TilePixels, rows = frame.Size.Y / TilePixels;
        if (written.Cell % RowCells + columns > RowCells || written.Cell + (rows - 1) * RowCells + columns > laid.Length)
            throw new InvalidDataException($"{Sheet.Name} picture {written.Picture} written at cell {written.Cell} runs off its tilemap.");
        for (int row = 0; row < rows; row++)
        for (int column = 0; column < columns; column++)
        {
            laid[written.Cell + row * RowCells + column] = new BgTilePicture(frame,
                new Position16<ImagePixels>(CoordinateAxis.Wrap(column * TilePixels), CoordinateAxis.Wrap(row * TilePixels)));
        }
    }
}
