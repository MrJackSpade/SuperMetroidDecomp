namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectNoobTubeVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectNoobTubeVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectNoobTubeVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectNoobTubeVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
