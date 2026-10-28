using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Development-tool instance members of <see cref="BombSpreadHorizontalVelocity"/>.</summary>
internal static class BombSpreadHorizontalVelocityToolingExtensions
{
    /// <summary>The high-byte bit of the packed launch word that sends the bomb left.</summary>
    internal const int LeftwardBit = 128;

    /// <summary>Development-tool instance members of <see cref="BombSpreadHorizontalVelocity"/>.</summary>
    extension(BombSpreadHorizontalVelocity self)
    {
        /// <summary>The velocity in the launch definition's packed form, 0 through 65535, for traces that print it.</summary>
        /// <summary>Decodes the cartridge's packed launch word: whole pixels and the leftward bit high, 256ths of a pixel low.</summary>
        public static BombSpreadHorizontalVelocity FromLaunchWord(int launchWord)
        {
            int high = UnsignedEightEight.Whole(launchWord);
            bool movingLeft = high >= LeftwardBit;
            return BombSpreadHorizontalVelocity.Of(movingLeft, movingLeft ? high - LeftwardBit : high, UnsignedEightEight.Fraction(launchWord));
        }

        public int LaunchWord =>
            LittleEndianBytes.Word(checked((byte)self.FractionSteps), checked((byte)(self.WholePixels + (self.MovingLeft ? LeftwardBit : 0))));
    }
}
