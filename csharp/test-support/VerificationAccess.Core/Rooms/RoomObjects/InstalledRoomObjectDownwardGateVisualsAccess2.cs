namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectDownwardGateVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectDownwardGateVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectDownwardGateVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectDownwardGateVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
