namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectSamusEaterVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectSamusEaterVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectSamusEaterVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectSamusEaterVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
