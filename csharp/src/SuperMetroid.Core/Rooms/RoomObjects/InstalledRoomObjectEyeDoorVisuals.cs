using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the eye doors' eye, middle, bottom and clearing frames (#1557); the opening clear shows the
/// shared clear's picture mirrored. Door terrain, attack logic, hit counters, timers and persistence stay the game's.
/// </summary>
public sealed class InstalledRoomObjectEyeDoorVisuals
{
    /// <summary>The shared clear's picture mirrored left to right, which the right door's opening clear shows.</summary>
    private readonly RoomObjectFrameImage mirroredClear;

    /// <summary>Holds the pictures the import made of every eye-door frame but the mirrored opening clear.</summary>
    /// <param name="images">Every eye-door frame's picture but the mirrored opening clear's, keyed by its draw.</param>
    /// <exception cref="InvalidDataException">The shared clear is not pictured.</exception>
    internal InstalledRoomObjectEyeDoorVisuals(RoomObjectFrameImages<RoomObjectDraw> images)
    {
        Images = Ensure.NotNull(images);
        mirroredClear = images.Image(EyeDoorObjectDrawDefinitions.SharedClear).MirroredHorizontally();
    }

    /// <summary>Every eye-door frame's picture but the mirrored opening clear's, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows; the mirrored opening clear shows the shared clear mirrored.</summary>
    /// <exception cref="InvalidDataException">The draw is not an eye-door frame, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw)
    {
        if (!EyeDoorDraws.TryDescribe(draw, out _))
            throw new InvalidDataException($"Eye-door visuals lack frame {draw}.");
        return EyeDoorDraws.VisualSource(draw) == draw ? Images.Image(draw) : mirroredClear;
    }
}
