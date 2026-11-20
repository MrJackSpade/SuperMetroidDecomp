namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectCollectibleVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectCollectibleVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectCollectibleVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectCollectibleVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
