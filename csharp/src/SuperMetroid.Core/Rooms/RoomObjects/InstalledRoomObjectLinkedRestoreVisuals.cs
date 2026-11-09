namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the bomb and contact-crumble restoration frames (#1557). The frames' terrain keeps its
/// linked parent and child collision.
/// </summary>
public sealed class InstalledRoomObjectLinkedRestoreVisuals
{
    /// <summary>Holds the pictures the import made of every restoration frame.</summary>
    /// <param name="images">Every restoration frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectLinkedRestoreVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every restoration frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        LinkedRestoreObjectFamily.TryDescribeAny(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Linked restoration visuals lack frame {draw}.");
}
