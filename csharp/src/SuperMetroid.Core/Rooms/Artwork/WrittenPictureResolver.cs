using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what pictures written over a room's layers show (#1392, #1466): each written picture's frame in the room's
/// written picture sheet for the values the appearance inputs it depends on take now, where it was written (#1514); and,
/// for the effect overlay, the pictures of its cells.
/// </summary>
public sealed class WrittenPictureResolver(WrittenPictureSheet sheet)
{
    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = ScreenGeometry.BackgroundTileSizePixels;

    /// <summary>What was last resolved: an overlay or an effect overlay plane.</summary>
    private object? source;

    /// <summary>The frame each written picture showed when last resolved.</summary>
    private SpriteSheetFrame[] shown = [];

    /// <summary>Each cell's picture as last resolved for a plane, null where nothing is written.</summary>
    private ImmutableArray<BgTilePicture?> cells;

    /// <summary>The written pictures as last resolved for an overlay.</summary>
    private ImmutableArray<PlacedWrittenPicture> placedPictures;

    /// <summary>The sheet.</summary>
    public WrittenPictureSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>
    /// Each picture written in <paramref name="overlay"/>, in the order written, as its frame while the room's inputs take
    /// the values <paramref name="appearance"/> reads, where it was written.
    /// </summary>
    /// <exception cref="InvalidOperationException">The sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures a written picture for no such values.</exception>
    public ImmutableArray<PlacedWrittenPicture> Resolve(RoomWrittenOverlay overlay, RoomAppearanceView appearance)
    {
        Ensure.NotNull(overlay);
        if (Sheet.Layer != WrittenPictureLayer.WrittenOverRoom)
            throw new InvalidOperationException($"{Sheet.Name} pictures nothing written over the room's layers.");
        SpriteSheetFrame[] frames = Frames(null, overlay.Pictures, appearance);
        if (Unchanged(overlay, frames))
            return placedPictures;
        var placed = ImmutableArray.CreateBuilder<PlacedWrittenPicture>(frames.Length);
        for (int index = 0; index < frames.Length; index++)
            placed.Add(new PlacedWrittenPicture(frames[index], overlay.Pictures[index].Cell));
        source = overlay;
        shown = frames;
        placedPictures = placed.MoveToImmutable();
        return placedPictures;
    }

    /// <summary>
    /// Each cell's picture of a <paramref name="plane"/>-sized plane: <paramref name="fill"/>'s one-tile picture, or nothing
    /// when it is null, with <paramref name="placed"/> laid over the cells in order, while the room's inputs take the values
    /// <paramref name="appearance"/> reads; re-laid only when <paramref name="placement"/> is another than last resolved or a
    /// picture shows another frame. The layer that owns the placement gives it, as the effect overlay's do.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The sheet pictures a written picture for no such values, the fill is not one tile, or a picture runs off the plane.
    /// </exception>
    internal ImmutableArray<BgTilePicture?> ResolvePlaced(object placement, Extent16<TilemapCells> plane, string? fill, IReadOnlyList<WrittenPicture> placed,
        RoomAppearanceView appearance)
    {
        Ensure.NotNull(placed);
        SpriteSheetFrame[] frames = Frames(fill, placed, appearance);
        if (Unchanged(placement, frames))
            return cells;
        int filled = fill is null ? 0 : 1;
        var next = new BgTilePicture?[plane.Area];
        if (fill is not null)
        {
            if (frames[0].Size.X != TilePixels || frames[0].Size.Y != TilePixels)
                throw new InvalidDataException($"{Sheet.Name} pictures {fill} as more than one tile.");
            Array.Fill(next, new BgTilePicture(frames[0], default));
        }
        for (int index = 0; index < placed.Count; index++)
            Lay(next, plane, placed[index], frames[filled + index]);
        source = placement;
        shown = frames;
        cells = [.. next];
        return cells;
    }

    /// <summary>The frames <paramref name="fill"/>, when given, and then each of <paramref name="placed"/> show now.</summary>
    private SpriteSheetFrame[] Frames(string? fill, IReadOnlyList<WrittenPicture> placed, RoomAppearanceView appearance)
    {
        int filled = fill is null ? 0 : 1;
        var frames = new SpriteSheetFrame[filled + placed.Count];
        if (fill is not null)
            frames[0] = Sheet.Picture(fill, appearance);
        for (int index = 0; index < placed.Count; index++)
            frames[filled + index] = Sheet.Picture(placed[index].Picture, appearance);
        return frames;
    }

    /// <summary>Whether <paramref name="placement"/> was last resolved and showed <paramref name="frames"/>.</summary>
    private bool Unchanged(object placement, SpriteSheetFrame[] frames)
    {
        bool unchanged = ReferenceEquals(placement, source) && shown.Length == frames.Length;
        for (int index = 0; unchanged && index < frames.Length; index++)
            unchanged = ReferenceEquals(frames[index], shown[index]);
        return unchanged;
    }

    /// <summary>Lays each tile of <paramref name="frame"/> over the cells <paramref name="written"/> covers from its cell.</summary>
    /// <exception cref="InvalidDataException">The picture runs past the plane's edge.</exception>
    private void Lay(BgTilePicture?[] laid, Extent16<TilemapCells> plane, WrittenPicture written, SpriteSheetFrame frame)
    {
        int columns = frame.Size.X / TilePixels, rows = frame.Size.Y / TilePixels;
        if (written.Cell.X + columns > plane.X || written.Cell.Y + rows > plane.Y)
            throw new InvalidDataException($"{Sheet.Name} picture {written.Picture} written at cell ({written.Cell.X}, {written.Cell.Y}) runs off its plane.");
        for (int row = 0; row < rows; row++)
        for (int column = 0; column < columns; column++)
        {
            laid[(written.Cell.Y + row) * plane.X + written.Cell.X + column] = new BgTilePicture(frame,
                new Position16<ImagePixels>(CoordinateAxis.Wrap(column * TilePixels), CoordinateAxis.Wrap(row * TilePixels)));
        }
    }
}
