namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectGrappleBlockVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectGrappleBlockVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectGrappleBlockVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectGrappleBlockVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
