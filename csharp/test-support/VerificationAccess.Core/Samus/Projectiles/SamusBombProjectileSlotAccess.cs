
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusBombProjectileSlot"/> members production does not use.</summary>
internal static class SamusBombProjectileSlotAccess
{
    /// <summary>Recognizes an active bomb whose timer-zero slot owns an explosion or Power Bomb terrain scan.</summary>
    extension(SamusBombProjectileSlot self)
    {
        /// <summary>
        /// True after a normal bomb selects `$93:A06B`, or while a Power Bomb's timer-zero
        /// slot owns the expanding bank-$88 terrain scan, and before native-style deletion.
        /// </summary>
        internal bool IsExploding => self.IsActive && self.BombTimer.IsZero;
    }
}
