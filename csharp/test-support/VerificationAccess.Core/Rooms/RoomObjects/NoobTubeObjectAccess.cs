namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="NoobTubeObject"/> members production does not use.</summary>
internal static class NoobTubeObjectAccess
{
    /// <summary>Exposes <see cref="NoobTubeObject"/> state that only verification reads.</summary>
    extension(NoobTubeObject self)
    {
        /// <summary>The strike on the tube's block it has yet to take, or null for none.</summary>
        internal ResidentObjectProjectileHit? AwaitingStrike => self.strike.Awaiting;
    }
}
