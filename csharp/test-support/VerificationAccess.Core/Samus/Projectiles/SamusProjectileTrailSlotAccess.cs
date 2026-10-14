
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusProjectileTrailSlot"/> members production does not use.</summary>
internal static class SamusProjectileTrailSlotAccess
{
    /// <summary>Maps a trail slot to its native two-byte offset and left-stream active marker.</summary>
    extension(SamusProjectileTrailSlot self)
    {
        /// <summary>Converts the trail slot index to its two-byte native slot offset.</summary>
        internal int NativeByteIndex => self.SlotIndex * 2;

        /// <summary>The left trail stream owns the slot while its instruction timer is active.</summary>
        internal bool IsActive => !self.Left.InstructionTimer.IsZero;
    }
}
