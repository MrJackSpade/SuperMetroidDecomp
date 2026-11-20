namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectShotBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectShotBlockVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectShotBlockVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectShotBlockVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
