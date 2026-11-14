using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what pictures written over a room's layers show (#1392, #1466): each written picture's frame in the room's
/// written picture sheet for the values the appearance inputs it depends on take now, where it was written (#1514). The
/// effect overlay's page is resolved from the same kind of sheet.
/// </summary>
public sealed class WrittenPictureResolver(WrittenPictureSheet sheet)
{
    /// <summary>The overlay last resolved.</summary>
    private RoomWrittenOverlay? source;

    /// <summary>The picture each written picture showed when last resolved.</summary>
    private LayeredPicture[] shown = [];

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
        LayeredPicture[] frames = Frames(overlay.Pictures, appearance);
        if (Unchanged(overlay, frames))
            return placedPictures;
        var placed = ImmutableArray.CreateBuilder<PlacedWrittenPicture>(frames.Length);
        for (int index = 0; index < frames.Length; index++)
            placed.Add(new PlacedWrittenPicture(frames[index], overlay.Pictures[index].Corner));
        source = overlay;
        shown = frames;
        placedPictures = placed.MoveToImmutable();
        return placedPictures;
    }

    /// <summary>The pictures each of <paramref name="placed"/> shows now.</summary>
    private LayeredPicture[] Frames(IReadOnlyList<WrittenPicture> placed, RoomAppearanceView appearance)
    {
        var frames = new LayeredPicture[placed.Count];
        for (int index = 0; index < placed.Count; index++)
            frames[index] = Sheet.Picture(placed[index].Picture, appearance);
        return frames;
    }

    /// <summary>Whether <paramref name="overlay"/> was last resolved and showed <paramref name="frames"/>.</summary>
    private bool Unchanged(RoomWrittenOverlay overlay, LayeredPicture[] frames)
    {
        bool unchanged = ReferenceEquals(overlay, source) && shown.Length == frames.Length;
        for (int index = 0; unchanged && index < frames.Length; index++)
            unchanged = Equals(frames[index], shown[index]);
        return unchanged;
    }
}
