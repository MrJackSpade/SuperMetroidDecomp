namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectKraidVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectKraidVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectKraidVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectKraidVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
