namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Development-tool instance members of <see cref="SamusBombProjectileSlot"/>.</summary>
internal static class SamusBombProjectileSlotToolingExtensions
{
    /// <summary>Exposes the spread bomb's launch and velocity words to tools and verification.</summary>
    extension(SamusBombProjectileSlot self)
    {
        /// <summary>
        /// The spread bomb's horizontal travel in its launch definition's packed form, 0 through 65535, for traces that
        /// print it.
        /// </summary>
        public int BombSpreadDirectionMagnitude => self.BombSpreadHorizontal.LaunchWord;
        /// <summary>The whole word of <see cref="SamusBombProjectileSlot.BombSpreadVerticalVelocity"/>, 0 through 65535, for traces that print it.</summary>
        public int BombSpreadYVelocity => self.BombSpreadVerticalVelocity.WholeWord;
        /// <summary>The fraction word of <see cref="SamusBombProjectileSlot.BombSpreadVerticalVelocity"/>, 0 through 65535, for traces that print it.</summary>
        public int BombSpreadYSubvelocity => self.BombSpreadVerticalVelocity.Fraction;
    }
}
