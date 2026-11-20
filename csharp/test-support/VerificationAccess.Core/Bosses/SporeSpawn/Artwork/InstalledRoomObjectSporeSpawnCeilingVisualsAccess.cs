using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Verification access to <see cref="InstalledRoomObjectSporeSpawnCeilingVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectSporeSpawnCeilingVisualsAccess
{
    /// <summary>Exposes <see cref="InstalledRoomObjectSporeSpawnCeilingVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectSporeSpawnCeilingVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
