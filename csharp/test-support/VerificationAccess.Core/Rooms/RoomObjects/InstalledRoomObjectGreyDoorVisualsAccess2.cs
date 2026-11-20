namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectGreyDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectGreyDoorVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectGreyDoorVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectGreyDoorVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
