namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectLinkedRestoreVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectLinkedRestoreVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectLinkedRestoreVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectLinkedRestoreVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
