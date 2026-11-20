namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSpeedBoosterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectSpeedBoosterVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectSpeedBoosterVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectSpeedBoosterVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
