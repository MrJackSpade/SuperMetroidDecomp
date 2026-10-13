using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Stateless enemy arithmetic shared by the bank-$A0 species routines: cartridge angles, eight-bit velocity
/// steps, the linear and quadratic speed tables, and eight-bit sine/cosine products.
/// </summary>
internal static class EnemyMath
{
    /// <summary>
    /// Ports <c>CalculateAngleOf_12_14_Offset</c> at $A0:C0AF. Zero points upward and the
    /// result advances clockwise in 256 units per turn, matching bank-$A0 enemy callers.
    /// </summary>
    internal static byte CalculateCartridgeAngle(Offset16<RoomPixels> delta)
    {
        int quadrant = 0;
        ushort absoluteX = unchecked((ushort)delta.X);
        ushort absoluteY = unchecked((ushort)delta.Y);
        if (delta.X < 0)
        {
            quadrant += 2;
            absoluteX = unchecked((ushort)-absoluteX);
        }
        if (delta.Y < 0)
        {
            quadrant++;
            absoluteY = unchecked((ushort)-absoluteY);
        }

        if (absoluteY < absoluteX)
        {
            // The routine chooses its octant using words, then writes only bytes
            // to the hardware divider. Preserve overflow rather than extending
            // its intended +/-255 range with a more accurate host calculation.
            int divided = SnesUnsignedDivision.Quotient(
                unchecked((ushort)SnesEightEight.FromWhole(absoluteY)), unchecked((byte)absoluteX));
            return quadrant switch
            {
                0 => unchecked((byte)((divided >> 3) + SnesAngle.QuarterTurn.TableIndex)),
                1 => unchecked((byte)(SnesAngle.QuarterTurn.TableIndex - (divided >> 3))),
                2 => unchecked((byte)(-SnesAngle.QuarterTurn.TableIndex - (divided >> 3))),
                _ => unchecked((byte)((divided >> 3) - SnesAngle.QuarterTurn.TableIndex)),
            };
        }

        int inverseDivided = SnesUnsignedDivision.Quotient(
            unchecked((ushort)SnesEightEight.FromWhole(absoluteX)), unchecked((byte)absoluteY));
        return quadrant switch
        {
            0 => unchecked((byte)(SnesAngle.HalfTurn.TableIndex - (inverseDivided >> 3))),
            1 => unchecked((byte)(inverseDivided >> 3)),
            2 => unchecked((byte)((inverseDivided >> 3) + SnesAngle.HalfTurn.TableIndex)),
            _ => unchecked((byte)(-(inverseDivided >> 3))),
        };
    }

    /// <summary>Adds one signed 8.8 velocity to a wrapped 16.16 position pair.</summary>
    internal static (ushort Position, ushort Subposition) AddEightBitVelocity(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        SnesFixedPosition result = new SnesSignedEightEight(velocity)
            .ToSixteenSixteenDelta()
            .AddTo(position, subposition);
        return (result.Whole, result.Fraction);
    }

    /// <summary>
    /// Sign-extends one native 8.8 velocity and shifts it into the signed 16.16 displacement
    /// consumed by the shared enemy collision routines. Rio-family AI uses this exact
    /// <c>INT16_SHL8</c> conversion on both axes.
    /// </summary>
    internal static int ToEightBitVelocityDisplacement(ushort velocity) =>
        new SnesSignedEightEight(velocity).ToSixteenSixteenDelta().RawValue;

    /// <summary>
    /// Returns the cartridge's modular absolute value of one signed 16-bit difference.
    /// Value $8000 intentionally remains $8000: the 65816 negation wraps, just like this
    /// unchecked host operation. Alcoon, Beetom, Work Robot, Polyp, and Namihe all consume
    /// this exact bank-$A0 distance convention.
    /// </summary>
    internal static ushort WrappedMagnitude(ushort value) =>
        unchecked((short)value) < 0 ? unchecked((ushort)-value) : value;

    /// <summary>
    /// Implements the shared bank-$A0 proximity helpers' strict <c>magnitude &lt; limit</c>
    /// test. Equality is outside, and subtraction happens in wrapped 16-bit world space.
    /// </summary>
    internal static bool IsWithinStrictModularDistance(
        ushort firstCoordinate,
        ushort secondCoordinate,
        ushort limit) =>
        WrappedMagnitude(unchecked((ushort)(firstCoordinate - secondCoordinate))) < limit;

    /// <summary>
    /// Reads one signed 16.16 entry from <c>CommonEnemySpeeds_LinearlyIncreasing</c>.
    /// <paramref name="byteOffset"/> is the native byte index, including the four-byte
    /// positive/negative half selector used by several enemy parameter formats.
    /// </summary>
    internal static (short Whole, ushort Fraction) ReadLinearEnemySpeed(ushort byteOffset) =>
        EnemyLinearSpeedDefinitions.Read(byteOffset);

    /// <summary>
    /// Reads one signed 16.16 entry from <c>CommonEnemySpeeds_QuadraticallyIncreasing</c>.
    /// The logical table index is multiplied by the native eight-byte record size; the
    /// negative half uses the separately stored, bug-compatible ROM negation rather than
    /// negating the positive host integer.
    /// </summary>
    internal static int ReadQuadraticEnemySpeed(ushort tableIndex, bool negative) =>
        EnemyQuadraticSpeedDefinitions.ReadDisplacement(
            tableIndex * EnemyQuadraticSpeedDefinitions.RecordSize + (negative ? 4 : 0));

    /// <summary>
    /// Ports the integer result of <c>EightBitSineMultiplication</c> at $A0:B0DA. The SNES
    /// routine multiplies an unsigned table byte by the low byte of radius, then negates the
    /// integer and fractional words independently for angles with bit seven set. Returning
    /// <c>-floor(product / 256)</c> therefore preserves its documented negative-fraction bug;
    /// <c>Math.Sin</c> or a normal fixed-point negation would disagree by one pixel.
    /// </summary>
    internal static int ReadEightBitSineProduct(ushort angle, ushort radius)
    {
        int byteAngle = unchecked((byte)angle);
        int sample = EnemyTrigonometryTables.EightBitHalfWave(byteAngle & 0x7f);
        int magnitude = SnesEightEight.Multiply(sample, SnesLittleEndian.LowByte(radius));
        return byteAngle < 0x80 ? magnitude : -magnitude;
    }

    /// <summary>Ports <c>EightBitCosineMultiplication</c> at $A0:B0B2.</summary>
    internal static int ReadEightBitCosineProduct(ushort angle, ushort radius) =>
        ReadEightBitSineProduct(
            unchecked((ushort)(angle + SnesAngle.QuarterTurn.TableIndex)),
            radius);

    /// <summary>Ports <c>EightBitNegativeSineMultiplication</c> at $A0:B0C6.</summary>
    internal static int ReadEightBitNegativeSineProduct(ushort angle, ushort radius) =>
        ReadEightBitSineProduct(
            unchecked((ushort)(angle + SnesAngle.HalfTurn.TableIndex)),
            radius);

    /// <summary>
    /// The point on a circle at an eight-bit angle: the cosine product as X and the negative-sine
    /// product as Y, each truncated to its native sixteen-bit word.
    /// </summary>
    internal static Offset16<RoomPixels> CircleOffset(ushort angle, ushort radius) => new(
        unchecked((short)ReadEightBitCosineProduct(angle, radius)),
        unchecked((short)ReadEightBitNegativeSineProduct(angle, radius)));

    /// <summary>
    /// Returns the complete signed 16.16 result produced by the native eight-bit sine
    /// multiplier, including its independent whole/fraction negation bug. Evir needs both
    /// words for projectile motion; callers that only need pixels use the helpers above.
    /// </summary>
    internal static (short Whole, ushort Fraction) ReadEightBitSineFixedProduct(
        ushort angle,
        ushort radius)
    {
        int byteAngle = unchecked((byte)angle);
        int sample = EnemyTrigonometryTables.EightBitHalfWave(byteAngle & 0x7f);
        ushort product = unchecked((ushort)(sample * (SnesLittleEndian.LowByte(radius))));
        ushort whole = unchecked((ushort)(SnesUnsignedEightEight.Whole(product)));
        ushort fraction = unchecked((ushort)SnesSixteenSixteen.FromEightEight(product));
        if (byteAngle >= 0x80)
        {
            // $A0:B11B and $B125 negate each word independently instead of carrying the
            // fractional increment into the whole word. Preserve that documented ROM bug.
            whole = unchecked((ushort)(~whole + 1));
            fraction = unchecked((ushort)(~fraction + 1));
        }
        return (unchecked((short)whole), fraction);
    }

    /// <summary>Rotates the native fixed sine multiplication by a quarter turn for cosine, retaining its negative-fraction behavior.</summary>
    internal static (short Whole, ushort Fraction) ReadEightBitCosineFixedProduct(
        ushort angle,
        ushort radius) =>
        ReadEightBitSineFixedProduct(
            unchecked((ushort)(angle + SnesAngle.QuarterTurn.TableIndex)),
            radius);

    /// <summary>Rotates the native fixed sine multiplication by a half turn to obtain negative sine.</summary>
    internal static (short Whole, ushort Fraction) ReadEightBitNegativeSineFixedProduct(
        ushort angle,
        ushort radius) =>
        ReadEightBitSineFixedProduct(
            unchecked((ushort)(angle + SnesAngle.HalfTurn.TableIndex)),
            radius);

    /// <summary>
    /// Ports one half of <c>Do_Some_Math_With_Sine_Cosine_Terrible_Label_Name</c> at
    /// $A0:B643. The routine reads a 16-bit unsigned quarter-circle sample, multiplies it
    /// by an unsigned 16-bit magnitude, and returns the complete 16.16 product. Callers
    /// supply either $40 (absolute cosine) or $80 (absolute sine) as the angle offset.
    /// Keeping sample generation deterministic avoids host floating-point rounding and makes the raw
    /// velocity words directly comparable with the SNES multiplication result.
    /// </summary>
    internal static int ReadUnsignedSineMagnitudeProduct(
        ushort angle,
        ushort magnitude,
        ushort angleOffset)
    {
        int tableIndex = unchecked((ushort)(angle + angleOffset)) & 0x007f;
        ushort sample = EnemyTrigonometryTables.UnsignedHalfWave(tableIndex);
        uint product = (uint)sample * magnitude;
        return unchecked((int)product);
    }

    /// <summary>The absolute cosine half of the angle-to-vector routine: the sample a quarter turn ahead.</summary>
    internal static int ReadAbsoluteCosineProduct(int angle, ushort magnitude) =>
        ReadUnsignedSineMagnitudeProduct(unchecked((ushort)angle), magnitude, SnesAngle.QuarterTurn.TableIndex);

    /// <summary>The absolute sine half of the angle-to-vector routine: the sample a half turn ahead.</summary>
    internal static int ReadAbsoluteSineProduct(int angle, ushort magnitude) =>
        ReadUnsignedSineMagnitudeProduct(unchecked((ushort)angle), magnitude, SnesAngle.HalfTurn.TableIndex);

    /// <summary>Whether a table angle's cosine component is negative: the native tests bit 7 of the angle plus a quarter turn.</summary>
    internal static bool CosineIsNegative(int angle) => IsInSecondHalfTurn(angle + SnesAngle.QuarterTurn.TableIndex);

    /// <summary>Whether a table angle's sine component is negative: the native tests bit 7 of the angle plus a half turn.</summary>
    internal static bool SineIsNegative(int angle) => IsInSecondHalfTurn(angle + SnesAngle.HalfTurn.TableIndex);

    /// <summary>Whether a table angle lies in the second half turn (table bit 7).</summary>
    private static bool IsInSecondHalfTurn(int tableAngle) => (tableAngle & SnesAngle.HalfTurn.TableIndex) != 0;

    /// <summary>Multiplies a signed sine-table sample by speed with the cartridge's fixed-point rounding.</summary>
    internal static ushort MultiplyCartridgeSinCos(ushort speed, byte angle)
    {
        return EnemyTrigonometryTables.MultiplySignedSine(speed, angle);
    }

    /// <summary>Tests the sign of the low sixteen bits as a 65816 word, ignoring higher host bits.</summary>
    internal static bool IsNegative16(int value) => (short)unchecked((ushort)value) < 0;
}
