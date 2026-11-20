using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Verification access to <see cref="InstalledRoomObjectCrocomireVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectCrocomireVisualsAccess
{
    /// <summary>Exposes <see cref="InstalledRoomObjectCrocomireVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectCrocomireVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
