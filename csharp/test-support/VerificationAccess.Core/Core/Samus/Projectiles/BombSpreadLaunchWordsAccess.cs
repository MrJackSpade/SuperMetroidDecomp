namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="BombSpreadLaunchWords"/> members production does not use.</summary>
internal static class BombSpreadLaunchWordsAccess
{
    /// <summary>Decodes the cartridge's packed spread launch words for checks that compare with them.</summary>
    extension(BombSpreadLaunchWords)
    {
        /// <summary>
        /// Decodes the cartridge's packed launch word: whole pixels and the leftward bit high, 256ths of a pixel low, as
        /// a velocity negative leftward.
        /// </summary>
        public static SignedSixteenSixteen Unpack(int launchWord)
        {
            int high = EightEightOracle.UnsignedWhole(launchWord);
            bool movingLeft = high >= BombSpreadLaunchWords.LeftwardBit;
            SignedSixteenSixteen speed = SignedSixteenSixteen.FromParts(
                movingLeft ? high - BombSpreadLaunchWords.LeftwardBit : high, unchecked((byte)launchWord) << 8);
            return movingLeft ? -speed : speed;
        }
    }
}
