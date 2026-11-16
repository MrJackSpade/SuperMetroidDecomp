namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Development-tool instance members of <see cref="SamusBomb"/>.</summary>
internal static class SamusBombToolingExtensions
{
    /// <summary>Exposes the spread bomb's launch and velocity words to tools and verification.</summary>
    extension(SamusBomb self)
    {
        /// <summary>
        /// The spread bomb's horizontal velocity in the launch table's packed form, 0 through 65535, for traces that print
        /// it: the speed's whole pixels with the leftward bit above its 256ths. A bomb with no horizontal speed packs as
        /// rightward.
        /// </summary>
        public int BombSpreadDirectionMagnitude => BombSpreadLaunchWords.Pack(self.BombSpreadHorizontalVelocity);
        /// <summary>The whole word of <see cref="SamusBomb.BombSpreadVerticalVelocity"/>, 0 through 65535, for traces that print it.</summary>
        public int BombSpreadYVelocity => self.BombSpreadVerticalVelocity.UnsignedWholePixels;
        /// <summary>The fraction word of <see cref="SamusBomb.BombSpreadVerticalVelocity"/>, 0 through 65535, for traces that print it.</summary>
        public int BombSpreadYSubvelocity => self.BombSpreadVerticalVelocity.Fraction;
    }
}
