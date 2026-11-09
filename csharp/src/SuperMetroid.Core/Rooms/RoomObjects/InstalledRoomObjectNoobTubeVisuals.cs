namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the n00b tube's frames (#1557); their terrain and crack sequence stay the game's.
/// </summary>
public sealed class InstalledRoomObjectNoobTubeVisuals
{
    /// <summary>Holds the pictures the import made of every tube frame.</summary>
    /// <param name="images">Every tube frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectNoobTubeVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every tube frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        NoobTubeObjectDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"N00b tube visuals lack frame {draw}.");
}
