using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The pictures of a warning an encounter shows in the room (#1346, #1348, #1466, #1514, #1601, #2573): the Ceres
/// self-destruct warning's EMERGENCY title, Japanese subtitle and typed lines in Ceres Ridley's room, at the foreground's
/// depth, and the Zebes time-bomb warning's typed lines in Mother Brain's room, at the background's. Each picture shows at
/// its place in the room, scrolling with it, and a later picture covers an earlier one where they overlap. Everything shown
/// stays shown until Samus leaves the room. Immutable; each picture shown publishes a copy.
/// </summary>
public sealed class RoomWarningText
{
    /// <summary>The overlay with nothing written.</summary>
    public static RoomWarningText Empty { get; } = new([]);

    /// <summary>Takes the pictures, in the order they were written.</summary>
    private RoomWarningText(ImmutableArray<WrittenPicture> pictures) => Pictures = pictures;

    /// <summary>The pictures, in the order they were written.</summary>
    public ImmutableArray<WrittenPicture> Pictures { get; }

    /// <summary>A copy with <paramref name="written"/> written over it.</summary>
    /// <exception cref="ArgumentException">The picture has no name.</exception>
    public RoomWarningText With(WrittenPicture written)
    {
        ArgumentException.ThrowIfNullOrEmpty(written.Picture);
        return new(Pictures.Add(written));
    }
}
