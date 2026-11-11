using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The pictures an encounter wrote over one of the room's layers (#1346, #1348, #1466, #1514, #1601): the Ceres
/// self-destruct warning's EMERGENCY title, Japanese subtitle and typed lines over Ceres Ridley's room's foreground, and
/// the Zebes time-bomb warning's typed lines over Mother Brain's room's background. Each picture shows at its pixel of the
/// layer, scrolling with it, and a later picture covers an earlier one where they overlap. Everything written stays shown,
/// over what the layer otherwise shows, until Samus leaves the room. Immutable; each write publishes a copy.
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
