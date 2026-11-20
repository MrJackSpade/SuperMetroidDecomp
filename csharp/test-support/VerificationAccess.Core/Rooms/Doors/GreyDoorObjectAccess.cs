using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification access to <see cref="GreyDoorObject"/> members production does not use.</summary>
internal static class GreyDoorObjectAccess
{
    /// <summary>Exposes <see cref="GreyDoorObject"/> state that only verification reads.</summary>
    extension(GreyDoorObject self)
    {
        /// <summary>The shot at the cap's block it has yet to take, or null for none.</summary>
        internal ResidentObjectProjectileHit? AwaitingStrike => self.strike.Awaiting;
    }
}
