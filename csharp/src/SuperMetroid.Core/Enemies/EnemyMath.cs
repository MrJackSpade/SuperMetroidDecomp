namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Stateless enemy arithmetic shared by the bank-$A0 species routines: cartridge angles, eight-bit velocity
/// steps and eight-bit sine/cosine products.
/// </summary>
internal static class EnemyMath
{
    /// <summary>
    /// Clamps a signed 16.16 velocity to plus or minus <paramref name="maximum"/> whole pixels: a whole part at or past
    /// the maximum becomes exactly the maximum, one below its negation exactly the negation.
    /// </summary>
    internal static int ClampWholeVelocity(int velocity, int maximum)
    {
        int whole = WordWrap.Signed(SixteenSixteen.WholeOf(velocity));
        if (whole >= maximum)
            return SixteenSixteen.FromWhole(maximum);
        if (whole < -maximum)
            return SixteenSixteen.FromWhole(-maximum);
        return velocity;
    }

    /// <summary>
    /// Ports <c>CalculateAngleOf_12_14_Offset</c> at $A0:C0AF. Zero points upward and the
    /// result advances clockwise in 256 units per turn, matching bank-$A0 enemy callers.
    /// </summary>
    internal static Bearing CalculateBearing(Offset16<RoomPixels> delta)
    {
        // A signed word's magnitude is at most 32,768, so the absolute values need no wrap.
        int quadrant = 0;
        int absoluteX = Math.Abs(delta.X);
        int absoluteY = Math.Abs(delta.Y);
        if (delta.X < 0)
            quadrant += 2;
        if (delta.Y < 0)
            quadrant++;

        if (absoluteY < absoluteX)
        {
            // The octant is chosen from the full distances; the slope keeps the original's limits, so offsets
            // beyond 255 pixels bend the angle (AimAngleSlope).
            int divided = AimAngleSlope.Of(absoluteY, absoluteX);
            return Bearing.Wrapped(quadrant switch
            {
                0 => (divided >> 3) + RotationAngle.QuarterTurn.TableIndex,
                1 => RotationAngle.QuarterTurn.TableIndex - (divided >> 3),
                2 => -RotationAngle.QuarterTurn.TableIndex - (divided >> 3),
                _ => (divided >> 3) - RotationAngle.QuarterTurn.TableIndex,
            });
        }

        int inverseDivided = AimAngleSlope.Of(absoluteX, absoluteY);
        return Bearing.Wrapped(quadrant switch
        {
            0 => RotationAngle.HalfTurn.TableIndex - (inverseDivided >> 3),
            1 => inverseDivided >> 3,
            2 => (inverseDivided >> 3) + RotationAngle.HalfTurn.TableIndex,
            _ => -(inverseDivided >> 3),
        });
    }

    /// <summary>Adds one signed 8.8 velocity to a wrapped 16.16 position.</summary>
    internal static FixedCoordinate AddEightBitVelocity(FixedCoordinate position, SignedEightEight velocity) =>
        velocity.ToSixteenSixteenDelta().AddTo(position.Whole, position.Fraction);

    /// <summary>
    /// Sign-extends one native 8.8 velocity and shifts it into the signed 16.16 displacement
    /// consumed by the shared enemy collision routines. Rio-family AI uses this exact
    /// <c>INT16_SHL8</c> conversion on both axes.
    /// </summary>
    internal static int ToEightBitVelocityDisplacement(SignedEightEight velocity) =>
        velocity.ToSixteenSixteenDelta().RawValue;

    /// <summary>
    /// Returns the cartridge's modular absolute value of one signed 16-bit difference.
    /// Value $8000 intentionally remains $8000: the 65816 negation wraps, just like this
    /// unchecked host operation. Alcoon, Beetom, Work Robot, Polyp, and Namihe all consume
    /// this exact bank-$A0 distance convention. <paramref name="value"/> is a word, 0 through 65535.
    /// </summary>
    internal static int WrappedMagnitude(int value) => Math.Abs(WordWrap.Signed(value));

    /// <summary>
    /// Implements the shared bank-$A0 proximity helpers' strict <c>magnitude &lt; limit</c>
    /// test. Equality is outside, and subtraction happens in wrapped 16-bit world space.
    /// </summary>
    internal static bool IsWithinStrictModularDistance(
        int firstCoordinate,
        int secondCoordinate,
        int limit) =>
        CoordinateAxis.Distance(firstCoordinate, secondCoordinate) < limit;

    /// <summary>
    /// Ports the integer result of <c>EightBitSineMultiplication</c> at $A0:B0DA. The SNES
    /// routine multiplies an unsigned table byte by the low byte of radius, then negates the
    /// integer and fractional words independently for angles with bit seven set. Returning
    /// <c>-floor(product / 256)</c> therefore preserves its documented negative-fraction bug;
    /// <c>Math.Sin</c> or a normal fixed-point negation would disagree by one pixel.
    /// </summary>
    internal static int ReadEightBitSineProduct(ByteAngle angle, int radius)
    {
        int sample = Trigonometry.EightBitHalfWave(angle.HalfWaveIndex);
        int magnitude = EightEight.Multiply(sample, RadiusMultiplierOperand(radius));
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>Ports <c>EightBitCosineMultiplication</c> at $A0:B0B2.</summary>
    internal static int ReadEightBitCosineProduct(ByteAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.AddTableUnits(ByteAngle.QuarterTurn.RawValue), radius);

    /// <summary>Ports <c>EightBitNegativeSineMultiplication</c> at $A0:B0C6.</summary>
    internal static int ReadEightBitNegativeSineProduct(ByteAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.AddTableUnits(ByteAngle.HalfTurn.RawValue), radius);

    /// <summary>
    /// The point on a circle at an eight-bit angle: the cosine product as X and the negative-sine
    /// product as Y, each truncated to its native sixteen-bit word.
    /// </summary>
    internal static Offset16<RoomPixels> CircleOffset(ByteAngle angle, int radius) => Offset16<RoomPixels>.Wrapped(
        ReadEightBitCosineProduct(angle, radius),
        ReadEightBitNegativeSineProduct(angle, radius));

    /// <summary>
    /// Returns the complete signed 16.16 result produced by the native eight-bit sine
    /// multiplier, including its independent whole/fraction negation bug. Evir needs both
    /// words for projectile motion; callers that only need pixels use the helpers above.
    /// </summary>
    internal static SignedSixteenSixteen ReadEightBitSineFixedProduct(
        ByteAngle angle,
        int radius)
    {
        // A half-wave byte times a radius byte is below 65,536, so the product needs no wrap.
        int sample = Trigonometry.EightBitHalfWave(angle.HalfWaveIndex);
        int product = sample * RadiusMultiplierOperand(radius);
        int whole = EightEight.WholeOf(product);
        int fraction = WordWrap.Unsigned(SixteenSixteen.FromEightEight(product));
        if (angle.IsInSecondHalf)
        {
            // $A0:B11B and $B125 negate each word independently instead of carrying the
            // fractional increment into the whole word. Preserve that documented ROM bug.
            whole = -whole;
            fraction = WordWrap.Unsigned(-fraction);
        }
        return SignedSixteenSixteen.FromParts(WordWrap.Signed(whole), fraction);
    }

    /// <summary>Rotates the native fixed sine multiplication by a quarter turn for cosine, retaining its negative-fraction behavior.</summary>
    internal static SignedSixteenSixteen ReadEightBitCosineFixedProduct(
        ByteAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.AddTableUnits(ByteAngle.QuarterTurn.RawValue), radius);

    /// <summary>Rotates the native fixed sine multiplication by a half turn to obtain negative sine.</summary>
    internal static SignedSixteenSixteen ReadEightBitNegativeSineFixedProduct(
        ByteAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.AddTableUnits(ByteAngle.HalfTurn.RawValue), radius);

    /// <summary>
    /// Ports one half of <c>Do_Some_Math_With_Sine_Cosine_Terrible_Label_Name</c> at
    /// $A0:B643. The routine reads a 16-bit unsigned quarter-circle sample, multiplies it
    /// by an unsigned 16-bit magnitude, and returns the complete 16.16 product. Callers
    /// supply either $40 (absolute cosine) or $80 (absolute sine) as the angle offset.
    /// Keeping sample generation deterministic avoids host floating-point rounding and makes the raw
    /// velocity words directly comparable with the SNES multiplication result.
    /// </summary>
    internal static int ReadUnsignedSineMagnitudeProduct(
        ByteAngle angle,
        int magnitude,
        int angleOffset)
    {
        int tableIndex = angle.AddTableUnits(angleOffset).HalfWaveIndex;
        int sample = Trigonometry.UnsignedHalfWave(tableIndex);
        uint product = (uint)sample * (uint)magnitude;
        return unchecked((int)product);
    }

    /// <summary>The absolute cosine half of the angle-to-vector routine: the sample a quarter turn ahead.</summary>
    internal static int ReadAbsoluteCosineProduct(ByteAngle angle, int magnitude) =>
        ReadUnsignedSineMagnitudeProduct(angle, magnitude, RotationAngle.QuarterTurn.TableIndex);

    /// <summary>The absolute sine half of the angle-to-vector routine: the sample a half turn ahead.</summary>
    internal static int ReadAbsoluteSineProduct(ByteAngle angle, int magnitude) =>
        ReadUnsignedSineMagnitudeProduct(angle, magnitude, RotationAngle.HalfTurn.TableIndex);

    /// <summary>Whether a table angle's cosine component is negative: the native tests bit 7 of the angle plus a quarter turn.</summary>
    internal static bool CosineIsNegative(ByteAngle angle) => angle.AddTableUnits(ByteAngle.QuarterTurn.RawValue).RawValue >= ByteAngle.HalfTurn.RawValue;

    /// <summary>Whether a table angle's sine component is negative: the native tests bit 7 of the angle plus a half turn.</summary>
    internal static bool SineIsNegative(ByteAngle angle) => angle.AddTableUnits(ByteAngle.HalfTurn.RawValue).RawValue >= ByteAngle.HalfTurn.RawValue;

    /// <summary>Multiplies a signed sine-table sample by speed with the cartridge's fixed-point rounding.</summary>
    internal static SignedEightEight MultiplySineSample(int speed, ByteAngle angle) =>
        Trigonometry.MultiplySignedSine(speed, angle);

    /// <summary>
    /// The cartridge's sign extension of a word's low byte (<c>Sign_Extend_A</c>): a negative low byte fills the high byte
    /// with ones; a positive one leaves whatever high byte the word already had, so a borrowed one survives (#1412).
    /// </summary>
    internal static int SignExtendLowByte(int word) =>
        WordWrap.SignedLowByte(word) < 0 ? FillHighByte(word) : Ensure.BetweenInclusive(0, ushort.MaxValue, word);

    /// <summary>The word whose low byte is <paramref name="value"/>'s and whose high byte is all ones: a negative 8-bit result (#1412).</summary>
    internal static int FillHighByte(int value) => WordWrap.Unsigned(value | ~byte.MaxValue);

    /// <summary>
    /// The eight-bit multiplier operand a radius word, 0 through 65535, supplies: the eight-bit sine multiplication
    /// takes only its low eight bits, so a radius of 256 or more wraps.
    /// </summary>
    private static int RadiusMultiplierOperand(int radius) => WordWrap.LowByte(Ensure.BetweenInclusive(0, ushort.MaxValue, radius));

    /// <summary>Tests the sign of the low sixteen bits as a 65816 word, ignoring higher host bits.</summary>
    internal static bool IsNegative16(int value) => WordWrap.IsNegative(value);
}
