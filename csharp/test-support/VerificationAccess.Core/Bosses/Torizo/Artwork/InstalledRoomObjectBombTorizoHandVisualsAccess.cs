using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>Verification access to <see cref="InstalledRoomObjectBombTorizoHandVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectBombTorizoHandVisualsAccess
{
    /// <summary>Exposes <see cref="InstalledRoomObjectBombTorizoHandVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectBombTorizoHandVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
