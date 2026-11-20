namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectStationVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectStationVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectStationVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectStationVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
