using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectEyeDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectEyeDoorVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectEyeDoorVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectEyeDoorVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows; the mirrored opening clear shows the shared clear mirrored.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
            Ensure.NotNull(draw) == EyeDoorObjectDrawDefinitions.MirroredOpeningClear ? self.mirroredClear : self.Images.Image(draw);
    }
}
