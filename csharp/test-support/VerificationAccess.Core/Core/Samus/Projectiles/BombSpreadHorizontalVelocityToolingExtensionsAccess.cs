namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="BombSpreadHorizontalVelocityToolingExtensions"/> members production does not use.</summary>
internal static class BombSpreadHorizontalVelocityToolingExtensionsAccess
{
    /// <summary>Exposes <see cref="BombSpreadHorizontalVelocityToolingExtensions"/> state that only verification reads.</summary>
    extension(BombSpreadHorizontalVelocity self)
    {
        /// <summary>The velocity in the launch definition's packed form, 0 through 65535, for traces that print it.</summary>
        /// <summary>Decodes the cartridge's packed launch word: whole pixels and the leftward bit high, 256ths of a pixel low.</summary>
        public static BombSpreadHorizontalVelocity FromLaunchWord(int launchWord)
        {
            int high = UnsignedEightEight.Whole(launchWord);
            bool movingLeft = high >= BombSpreadHorizontalVelocityToolingExtensions.LeftwardBit;
            return BombSpreadHorizontalVelocity.Of(movingLeft, movingLeft ? high - BombSpreadHorizontalVelocityToolingExtensions.LeftwardBit : high, UnsignedEightEight.Fraction(launchWord));
        }
    }
}
