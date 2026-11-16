using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomBlockBehavior"/> members production does not use.</summary>
internal static class RoomBlockBehaviorAccess
{
    /// <summary>Exposes <see cref="RoomBlockBehavior"/> state that only verification reads.</summary>
    extension(RoomBlockBehavior self)
    {
        /// <summary>Every reading of this behaviour.</summary>
        public RoomBlockReadings AllReadings => self.Readings;
    }
}
