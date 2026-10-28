namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Verification access to <see cref="RoomDoorBackground"/> members production does not use.</summary>
internal static class RoomDoorBackgroundAccess
{
    /// <summary>Exposes the door window's coverage, which the display packet draws and verification reads.</summary>
    extension(RoomDoorBackground self)
    {
        /// <summary>Whether the source screen still covers a scrolled position.</summary>
        /// <param name="scrolled">The scroll register plus the screen pixel.</param>
        public bool Covers(Position16<TilePlanePixels> scrolled)
        {
            Offset16<TilePlanePixels> within = scrolled - self.WindowOrigin;
            return (ushort)within.X < RoomDoorBackgroundConstants.WindowSize.X && (ushort)within.Y < RoomDoorBackgroundConstants.WindowSize.Y;
        }
    }
}
