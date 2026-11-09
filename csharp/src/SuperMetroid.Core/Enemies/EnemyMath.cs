namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Stateless enemy arithmetic shared by the species routines: byte angles, eight-bit velocity
/// steps and eight-bit sine/cosine products.
/// </summary>
internal static class EnemyMath
{
    /// <summary>
    /// Clamps a signed 16.16 velocity to plus or minus <paramref name="maximum"/> whole pixels: a whole part at or past
    /// the maximum becomes exactly the maximum, one below its negation exactly the negation.
    /// </summary>
    internal static SignedSixteenSixteen ClampWholeVelocity(SignedSixteenSixteen velocity, int maximum)
    {
        int whole = velocity.WholePixels;
        if (whole >= maximum)
            return SignedSixteenSixteen.FromWholePixels(maximum);
        if (whole < -maximum)
            return SignedSixteenSixteen.FromWholePixels(-maximum);
        return velocity;
    }

    /// <summary>
    /// The proportional pull a Metroid or Mochtroid takes from its velocity for a signed distance of
    /// <paramref name="distance"/> pixels from its target, -32768 through 32767: a quarter of the distance, rounded
    /// toward minus infinity, in 256ths of a pixel.
    /// </summary>
    internal static SignedSixteenSixteen HomingPull(int distance) =>
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(distance >> 2);

    /// <summary>
    /// Calculates the bearing of an offset. Zero points upward and the
    /// result advances clockwise in 256 units per turn, matching enemy callers.
    /// </summary>
    internal static Bearing CalculateBearing(Offset16<RoomPixels> delta)
    {
        // A signed word's magnitude reaches 32,768, which a short cannot hold: the offsets widen to int before their
        // absolute values are taken, so -32,768 gives 32,768 instead of overflowing.
        int quadrant = 0;
        int absoluteX = Math.Abs((int)delta.X);
        int absoluteY = Math.Abs((int)delta.Y);
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
                0 => (divided >> 3) + ByteAngle.QuarterTurn.RawValue,
                1 => ByteAngle.QuarterTurn.RawValue - (divided >> 3),
                2 => -ByteAngle.QuarterTurn.RawValue - (divided >> 3),
                _ => (divided >> 3) - ByteAngle.QuarterTurn.RawValue,
            });
        }

        int inverseDivided = AimAngleSlope.Of(absoluteX, absoluteY);
        return Bearing.Wrapped(quadrant switch
        {
            0 => ByteAngle.HalfTurn.RawValue - (inverseDivided >> 3),
            1 => inverseDivided >> 3,
            2 => (inverseDivided >> 3) + ByteAngle.HalfTurn.RawValue,
            _ => -(inverseDivided >> 3),
        });
    }

    /// <summary>Adds one signed 8.8 velocity to a wrapped 16.16 position.</summary>
    internal static FixedCoordinate AddEightBitVelocity(FixedCoordinate position, SignedEightEight velocity) =>
        velocity.ToSixteenSixteenDelta().AddTo(position.Whole, position.Fraction);

    /// <summary>
    /// Implements the shared proximity helpers' strict <c>magnitude &lt; limit</c>
    /// test. Equality is outside, and subtraction happens in wrapped 16-bit world space.
    /// </summary>
    internal static bool IsWithinStrictModularDistance(
        int firstCoordinate,
        int secondCoordinate,
        int limit) =>
        CoordinateAxis.Distance(firstCoordinate, secondCoordinate) < limit;

    /// <summary>
    /// Computes the integer result of the eight-bit sine multiplication. The
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

    /// <summary>Computes the eight-bit cosine multiplication.</summary>
    internal static int ReadEightBitCosineProduct(ByteAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.TurnedBySteps(ByteAngle.QuarterTurn.RawValue), radius);

    /// <summary>Computes the eight-bit negative sine multiplication.</summary>
    internal static int ReadEightBitNegativeSineProduct(ByteAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.TurnedBySteps(ByteAngle.HalfTurn.RawValue), radius);

    /// <summary>
    /// The point on a circle at an eight-bit angle: the cosine product as X and the negative-sine
    /// product as Y, each truncated to its sixteen-bit word.
    /// </summary>
    internal static Offset16<RoomPixels> CircleOffset(ByteAngle angle, int radius) => Offset16<RoomPixels>.Wrapped(
        ReadEightBitCosineProduct(angle, radius),
        ReadEightBitNegativeSineProduct(angle, radius));

    /// <summary>
    /// Returns the complete signed 16.16 result produced by the eight-bit sine
    /// multiplier, including its independent whole/fraction negation bug. Evir needs both
    /// words for projectile motion; callers that only need pixels use the helpers above.
    /// </summary>
    internal static SignedSixteenSixteen ReadEightBitSineFixedProduct(
        ByteAngle angle,
        int radius)
    {
        // A half-wave byte times a radius byte is below 65,536, so the product needs no wrap.
        int sample = Trigonometry.EightBitHalfWave(angle.HalfWaveIndex);
        SignedSixteenSixteen product = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(
            sample * RadiusMultiplierOperand(radius));
        // The game negates each word independently instead of carrying the
        // fractional increment into the whole word. Preserve that documented quirk.
        return angle.IsInSecondHalf ? product.NegatedWordsSeparately() : product;
    }

    /// <summary>Rotates the fixed sine multiplication by a quarter turn for cosine, retaining its negative-fraction behavior.</summary>
    internal static SignedSixteenSixteen ReadEightBitCosineFixedProduct(
        ByteAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.TurnedBySteps(ByteAngle.QuarterTurn.RawValue), radius);

    /// <summary>Rotates the fixed sine multiplication by a half turn to obtain negative sine.</summary>
    internal static SignedSixteenSixteen ReadEightBitNegativeSineFixedProduct(
        ByteAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.TurnedBySteps(ByteAngle.HalfTurn.RawValue), radius);

    /// <summary>
    /// Computes one half of the shared sine/cosine velocity product.
    /// It reads a 16-bit unsigned quarter-circle sample, multiplies it
    /// by an unsigned 16-bit magnitude, and returns the complete 16.16 product. Callers
    /// supply either 0x40 (absolute cosine) or 0x80 (absolute sine) as the angle offset.
    /// Keeping sample generation deterministic avoids host floating-point rounding and makes the raw
    /// velocity words directly comparable with the game's multiplication result.
    /// </summary>
    internal static int ReadUnsignedSineMagnitudeProduct(
        ByteAngle angle,
        int magnitude,
        int angleOffset)
    {
        int tableIndex = angle.TurnedBySteps(angleOffset).HalfWaveIndex;
        int sample = Trigonometry.UnsignedHalfWave(tableIndex);
        // A product that would not fit the multiplication's 32-bit result fails instead of wrapping.
        uint product = checked((uint)sample * (uint)Ensure.AtLeastZero(magnitude));
        return unchecked((int)product);
    }

    /// <summary>The absolute cosine half of the angle-to-vector routine: the sample a quarter turn ahead.</summary>
    internal static int ReadAbsoluteCosineProduct(ByteAngle angle, int magnitude) =>
        ReadUnsignedSineMagnitudeProduct(angle, magnitude, ByteAngle.QuarterTurn.RawValue);

    /// <summary>The absolute sine half of the angle-to-vector routine: the sample a half turn ahead.</summary>
    internal static int ReadAbsoluteSineProduct(ByteAngle angle, int magnitude) =>
        ReadUnsignedSineMagnitudeProduct(angle, magnitude, ByteAngle.HalfTurn.RawValue);

    /// <summary>
    /// The horizontal part of a speed of <paramref name="magnitude"/> 256ths of a pixel along <paramref name="angle"/>,
    /// regardless of direction: the absolute cosine product as an unsigned 16.16 speed.
    /// </summary>
    internal static UnsignedSixteenSixteen AbsoluteCosineSpeed(ByteAngle angle, int magnitude) =>
        UnsignedSixteenSixteen.FromRaw(unchecked((uint)ReadAbsoluteCosineProduct(angle, magnitude)));

    /// <summary>
    /// The vertical part of a speed of <paramref name="magnitude"/> 256ths of a pixel along <paramref name="angle"/>,
    /// regardless of direction: the absolute sine product as an unsigned 16.16 speed.
    /// </summary>
    internal static UnsignedSixteenSixteen AbsoluteSineSpeed(ByteAngle angle, int magnitude) =>
        UnsignedSixteenSixteen.FromRaw(unchecked((uint)ReadAbsoluteSineProduct(angle, magnitude)));

    /// <summary>Whether a table angle's cosine component is negative: the tests bit 7 of the angle plus a quarter turn.</summary>
    internal static bool CosineIsNegative(ByteAngle angle) => angle.TurnedBySteps(ByteAngle.QuarterTurn.RawValue).RawValue >= ByteAngle.HalfTurn.RawValue;

    /// <summary>Whether a table angle's sine component is negative: the tests bit 7 of the angle plus a half turn.</summary>
    internal static bool SineIsNegative(ByteAngle angle) => angle.TurnedBySteps(ByteAngle.HalfTurn.RawValue).RawValue >= ByteAngle.HalfTurn.RawValue;

    /// <summary>Multiplies a signed sine-table sample by speed with the game's fixed-point rounding.</summary>
    internal static SignedEightEight MultiplySineSample(int speed, ByteAngle angle) =>
        Trigonometry.MultiplySignedSine(speed, angle);

    /// <summary>The span of an eight-bit operand or result: values past it continue from zero.</summary>
    private const int ByteSpan = 256;

    /// <summary>
    /// The eight-bit multiplier operand a radius, zero or more, supplies: the eight-bit sine multiplication
    /// takes only its place within 256, so a radius of 256 or more continues from zero.
    /// </summary>
    private static int RadiusMultiplierOperand(int radius) => Ensure.AtLeastZero(radius) % ByteSpan;
}
