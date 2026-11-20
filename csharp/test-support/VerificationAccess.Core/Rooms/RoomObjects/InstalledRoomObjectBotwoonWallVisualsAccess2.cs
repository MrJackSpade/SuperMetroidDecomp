namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectBotwoonWallVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectBotwoonWallVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectBotwoonWallVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectBotwoonWallVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
