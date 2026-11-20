using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>Verification access to <see cref="InstalledRoomObjectChozoStatueVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectChozoStatueVisualsAccess
{
    /// <summary>Exposes <see cref="InstalledRoomObjectChozoStatueVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectChozoStatueVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
