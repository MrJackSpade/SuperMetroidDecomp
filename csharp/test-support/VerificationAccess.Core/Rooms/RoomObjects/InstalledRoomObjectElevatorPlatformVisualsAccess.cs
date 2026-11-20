namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectElevatorPlatformVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectElevatorPlatformVisualsAccess
{
    /// <summary>Exposes <see cref="InstalledRoomObjectElevatorPlatformVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectElevatorPlatformVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
