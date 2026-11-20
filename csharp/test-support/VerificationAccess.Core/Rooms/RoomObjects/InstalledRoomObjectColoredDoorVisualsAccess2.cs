namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectColoredDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectColoredDoorVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectColoredDoorVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectColoredDoorVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
