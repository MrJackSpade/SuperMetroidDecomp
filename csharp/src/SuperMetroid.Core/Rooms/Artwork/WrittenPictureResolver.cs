using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what pictures written over a room's layers show (#1392, #1466): each written picture's frame in the room's
/// written picture sheet for the values the appearance inputs it depends on take now, where it was written (#1514). The
/// effect overlay's page is resolved from the same kind of sheet (<c>FxOverlayPictureResolution</c>).
/// </summary>
public sealed class WrittenPictureResolver(WrittenPictureSheet sheet)
{
    /// <summary>The overlay last resolved.</summary>
    private RoomWrittenOverlay? source;

    /// <summary>The frame each written picture showed when last resolved.</summary>
    private SpriteSheetFrame[] shown = [];

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
        SpriteSheetFrame[] frames = Frames(overlay.Pictures, appearance);
        if (Unchanged(overlay, frames))
            return placedPictures;
        // Each picture shows where the cell it was written at lies on the layer.
        const int cellPixels = ScreenGeometry.BackgroundTileSizePixels;
        var placed = ImmutableArray.CreateBuilder<PlacedWrittenPicture>(frames.Length);
        for (int index = 0; index < frames.Length; index++)
        {
            Position16<TilemapCells> cell = overlay.Pictures[index].Cell;
            placed.Add(new PlacedWrittenPicture(frames[index], new Position16<TilePlanePixels>(cell.X * cellPixels, cell.Y * cellPixels)));
        }
        source = overlay;
        shown = frames;
        placedPictures = placed.MoveToImmutable();
        return placedPictures;
    }

    /// <summary>The frames each of <paramref name="placed"/> shows now.</summary>
    private SpriteSheetFrame[] Frames(IReadOnlyList<WrittenPicture> placed, RoomAppearanceView appearance)
    {
        var frames = new SpriteSheetFrame[placed.Count];
        for (int index = 0; index < placed.Count; index++)
            frames[index] = Sheet.Picture(placed[index].Picture, appearance);
        return frames;
    }

    /// <summary>Whether <paramref name="overlay"/> was last resolved and showed <paramref name="frames"/>.</summary>
    private bool Unchanged(RoomWrittenOverlay overlay, SpriteSheetFrame[] frames)
    {
        bool unchanged = ReferenceEquals(overlay, source) && shown.Length == frames.Length;
        for (int index = 0; unchanged && index < frames.Length; index++)
            unchanged = ReferenceEquals(frames[index], shown[index]);
        return unchanged;
    }
}
