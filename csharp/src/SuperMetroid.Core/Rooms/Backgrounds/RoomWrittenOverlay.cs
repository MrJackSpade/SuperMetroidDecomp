using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The pictures a room's actors wrote over a layer (#1346, #1348, #1466): the Ceres escape warning over the foreground,
/// and the escape typewriter's text over the foreground (Ceres) or the background (Zebes), in the order they were written,
/// each where it shows on the layer (#1514). A later picture covers an earlier one where they overlap. They stay, over
/// what the layer otherwise shows, until the next room. Immutable; each write publishes a copy.
/// </summary>
public sealed class RoomWrittenOverlay
{
    /// <summary>The overlay with nothing written.</summary>
    public static RoomWrittenOverlay Empty { get; } = new([]);

    /// <summary>Takes the pictures, in the order they were written.</summary>
    private RoomWrittenOverlay(ImmutableArray<WrittenPicture> pictures) => Pictures = pictures;

    /// <summary>The pictures, in the order they were written.</summary>
    public ImmutableArray<WrittenPicture> Pictures { get; }

    /// <summary>A copy with <paramref name="written"/> written over it.</summary>
    /// <exception cref="ArgumentException">The picture has no name.</exception>
    public RoomWrittenOverlay With(WrittenPicture written)
    {
        ArgumentException.ThrowIfNullOrEmpty(written.Picture);
        return new(Pictures.Add(written));
    }
}
