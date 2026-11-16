using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Spatial;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The cartridge's packed form of a spread bomb's horizontal velocity, for traces that print it: the speed's whole
/// pixels, 0 through 127, with the leftward bit in the high byte, and its 256ths of a pixel in the low byte.
/// </summary>
internal static class BombSpreadLaunchWords
{
    /// <summary>The high-byte bit of the packed launch word that sends the bomb left.</summary>
    internal const int LeftwardBit = 128;

    /// <summary>Packs <paramref name="velocity"/>; a velocity of nothing packs as rightward.</summary>
    /// <exception cref="OverflowException">The speed has 128 whole pixels or more, or finer than 256ths of a pixel.</exception>
    internal static int Pack(SignedSixteenSixteen velocity)
    {
        SignedSixteenSixteen speed = velocity.IsNegative ? -velocity : velocity;
        if ((speed.Fraction & byte.MaxValue) != 0)
            throw new OverflowException($"Spread bomb velocity {velocity} is finer than a 256th of a pixel.");
        return LittleEndianBytes.Word(
            checked((byte)(speed.Fraction >> 8)),
            checked((byte)(checked((sbyte)speed.WholePixels) + (velocity.IsNegative ? LeftwardBit : 0))));
    }
}
