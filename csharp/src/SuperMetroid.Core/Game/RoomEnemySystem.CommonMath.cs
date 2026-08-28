namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared fixed-point and angle primitives used by cartridge enemy families. These retain
/// the SNES's word wrapping and coarse integer division instead of substituting host floats.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const int LinearEnemySpeedTable = 0xa08187;
    private const int QuadraticEnemySpeedTable = 0xa0838f;
    private const int SharedEightBitSineTable = 0xa0b143;
    private const int SharedUnsignedSineTable = 0xa0b7ee;

    /// <summary>
    /// Ports <c>CalculateAngleOf_12_14_Offset</c> at $A0:C0AF. Zero points upward and the
    /// result advances clockwise in 256 units per turn, matching bank-$A0 enemy callers.
    /// </summary>
    private static byte CalculateCartridgeAngle(short x, short y)
    {
        int quadrant = 0;
        ushort absoluteX = unchecked((ushort)x);
        ushort absoluteY = unchecked((ushort)y);
        if (x < 0)
        {
            quadrant += 2;
            absoluteX = unchecked((ushort)-absoluteX);
        }
        if (y < 0)
        {
            quadrant++;
            absoluteY = unchecked((ushort)-absoluteY);
        }

        if (absoluteY < absoluteX)
        {
            int divided = absoluteX == 0 ? 0 : (absoluteY << 8) / absoluteX;
            return quadrant switch
            {
                0 => unchecked((byte)((divided >> 3) + 64)),
                1 => unchecked((byte)(64 - (divided >> 3))),
                2 => unchecked((byte)(-64 - (divided >> 3))),
                _ => unchecked((byte)((divided >> 3) - 64)),
            };
        }

        int inverseDivided = absoluteY == 0 ? 0 : (absoluteX << 8) / absoluteY;
        return quadrant switch
        {
            0 => unchecked((byte)(128 - (inverseDivided >> 3))),
            1 => unchecked((byte)(inverseDivided >> 3)),
            2 => unchecked((byte)((inverseDivided >> 3) + 128)),
            _ => unchecked((byte)(-(inverseDivided >> 3))),
        };
    }

    /// <summary>Adds one signed 8.8 velocity to a wrapped 16.16 position pair.</summary>
    private static (ushort Position, ushort Subposition) AddEightBitVelocity(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        int fixedPosition = (position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (unchecked((short)velocity) << 8));
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    /// <summary>
    /// Returns the cartridge's modular absolute value of one signed 16-bit difference.
    /// Value $8000 intentionally remains $8000: the 65816 negation wraps, just like this
    /// unchecked host operation. Alcoon, Beetom, Work Robot, Polyp, and Namihe all consume
    /// this exact bank-$A0 distance convention.
    /// </summary>
    private static ushort WrappedMagnitude(ushort value) =>
        unchecked((short)value) < 0 ? unchecked((ushort)-value) : value;

    /// <summary>
    /// Implements the shared bank-$A0 proximity helpers' strict <c>magnitude &lt; limit</c>
    /// test. Equality is outside, and subtraction happens in wrapped 16-bit world space.
    /// </summary>
    private static bool IsWithinStrictModularDistance(
        ushort firstCoordinate,
        ushort secondCoordinate,
        ushort limit) =>
        WrappedMagnitude(unchecked((ushort)(firstCoordinate - secondCoordinate))) < limit;

    /// <summary>
    /// Reads one signed 16.16 entry from <c>CommonEnemySpeeds_LinearlyIncreasing</c>.
    /// <paramref name="byteOffset"/> is the native byte index, including the four-byte
    /// positive/negative half selector used by several enemy parameter formats.
    /// </summary>
    private (short Whole, ushort Fraction) ReadLinearEnemySpeed(ushort byteOffset) =>
        (
            unchecked((short)ReadWord(_bus!, LinearEnemySpeedTable + byteOffset)),
            ReadWord(_bus!, LinearEnemySpeedTable + byteOffset + 2));

    /// <summary>
    /// Reads one signed 16.16 entry from <c>CommonEnemySpeeds_QuadraticallyIncreasing</c>.
    /// The logical table index is multiplied by the native eight-byte record size; the
    /// negative half uses the separately stored, bug-compatible ROM negation rather than
    /// negating the positive host integer.
    /// </summary>
    private int ReadQuadraticEnemySpeed(ushort tableIndex, bool negative)
    {
        int entryAddress = QuadraticEnemySpeedTable + tableIndex * 8;
        int componentOffset = negative ? 4 : 0;
        ushort subvelocity = ReadWord(_bus!, entryAddress + componentOffset);
        short wholeVelocity = unchecked((short)ReadWord(
            _bus!,
            entryAddress + componentOffset + 2));
        return unchecked((wholeVelocity << 16) | subvelocity);
    }

    /// <summary>
    /// Ports the integer result of <c>EightBitSineMultiplication</c> at $A0:B0DA. The SNES
    /// routine multiplies an unsigned table byte by the low byte of radius, then negates the
    /// integer and fractional words independently for angles with bit seven set. Returning
    /// <c>-floor(product / 256)</c> therefore preserves its documented negative-fraction bug;
    /// <c>Math.Sin</c> or a normal fixed-point negation would disagree by one pixel.
    /// </summary>
    private int ReadEightBitSineProduct(ushort angle, ushort radius)
    {
        int byteAngle = angle & 0xff;
        int sample = _bus!.ReadByte(SharedEightBitSineTable + (byteAngle & 0x7f));
        int magnitude = sample * (radius & 0xff) >> 8;
        return byteAngle < 0x80 ? magnitude : -magnitude;
    }

    /// <summary>Ports <c>EightBitCosineMultiplication</c> at $A0:B0B2.</summary>
    private int ReadEightBitCosineProduct(ushort angle, ushort radius) =>
        ReadEightBitSineProduct(unchecked((ushort)(angle + 0x40)), radius);

    /// <summary>Ports <c>EightBitNegativeSineMultiplication</c> at $A0:B0C6.</summary>
    private int ReadEightBitNegativeSineProduct(ushort angle, ushort radius) =>
        ReadEightBitSineProduct(unchecked((ushort)(angle + 0x80)), radius);

    /// <summary>
    /// Ports one half of <c>Do_Some_Math_With_Sine_Cosine_Terrible_Label_Name</c> at
    /// $A0:B643. The routine reads a 16-bit unsigned quarter-circle sample, multiplies it
    /// by an unsigned 16-bit magnitude, and returns the complete 16.16 product. Callers
    /// supply either $40 (absolute cosine) or $80 (absolute sine) as the angle offset.
    /// Keeping this table-backed avoids host floating-point rounding and makes the raw
    /// velocity words directly comparable with the SNES multiplication result.
    /// </summary>
    private int ReadUnsignedSineMagnitudeProduct(
        ushort angle,
        ushort magnitude,
        ushort angleOffset)
    {
        int tableIndex = unchecked((ushort)(angle + angleOffset)) & 0x007f;
        ushort sample = ReadWord(_bus!, SharedUnsignedSineTable + tableIndex * 2);
        uint product = (uint)sample * magnitude;
        return unchecked((int)product);
    }
}
