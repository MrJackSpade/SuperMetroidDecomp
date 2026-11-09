namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Stateless enemy arithmetic shared by the species routines: trigonometry angles, whole-pixel velocity
/// steps and sine/cosine products by radii of 0 through 255.
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
                0 => (divided >> 3) + TrigonometryAngle.QuarterTurn.RawValue,
                1 => TrigonometryAngle.QuarterTurn.RawValue - (divided >> 3),
                2 => -TrigonometryAngle.QuarterTurn.RawValue - (divided >> 3),
                _ => (divided >> 3) - TrigonometryAngle.QuarterTurn.RawValue,
            });
        }

        int inverseDivided = AimAngleSlope.Of(absoluteX, absoluteY);
        return Bearing.Wrapped(quadrant switch
        {
            0 => TrigonometryAngle.HalfTurn.RawValue - (inverseDivided >> 3),
            1 => inverseDivided >> 3,
            2 => (inverseDivided >> 3) + TrigonometryAngle.HalfTurn.RawValue,
            _ => -(inverseDivided >> 3),
        });
    }

    /// <summary>Adds one signed 8.8 velocity to a wrapped 16.16 position.</summary>
    internal static FixedCoordinate AddEightBitVelocity(FixedCoordinate position, SignedEightEight velocity) =>
        velocity.ToSixteenSixteenDelta().AddTo(position.Whole, position.Fraction);

    /// <summary>
    /// Implements the shared proximity helpers' strict <c>magnitude &lt; limit</c>
    /// test. Equality is outside, and subtraction wraps past 65535 in world space.
    /// </summary>
    internal static bool IsWithinStrictModularDistance(
        int firstCoordinate,
        int secondCoordinate,
        int limit) =>
        CoordinateAxis.Distance(firstCoordinate, secondCoordinate) < limit;

    /// <summary>
    /// Computes the integer result of the short sine multiplication. The
    /// game multiplies an unsigned table sample of 0 through 255 by the radius within 256, then negates the
    /// integer and fractional parts independently for angles in the second half of the turn. Returning
    /// <c>-floor(product / 256)</c> therefore preserves its documented negative-fraction bug;
    /// <c>Math.Sin</c> or a normal fixed-point negation would disagree by one pixel.
    /// </summary>
    internal static int ReadEightBitSineProduct(TrigonometryAngle angle, int radius)
    {
        int sample = Trigonometry.EightBitHalfWave(angle.HalfWaveIndex);
        int magnitude = EightEight.Multiply(sample, RadiusMultiplierOperand(radius));
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>Computes the short cosine multiplication.</summary>
    internal static int ReadEightBitCosineProduct(TrigonometryAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.TurnedBySteps(TrigonometryAngle.QuarterTurn.RawValue), radius);

    /// <summary>Computes the short negative sine multiplication.</summary>
    internal static int ReadEightBitNegativeSineProduct(TrigonometryAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.TurnedBySteps(TrigonometryAngle.HalfTurn.RawValue), radius);

    /// <summary>
    /// The point on a circle at a trigonometry angle: the cosine product as X and the negative-sine
    /// product as Y, each wrapped to 0 through 65535.
    /// </summary>
    internal static Offset16<RoomPixels> CircleOffset(TrigonometryAngle angle, int radius) => Offset16<RoomPixels>.Wrapped(
        ReadEightBitCosineProduct(angle, radius),
        ReadEightBitNegativeSineProduct(angle, radius));

    /// <summary>
    /// Returns the complete signed 16.16 result produced by the short sine
    /// multiplier, including its independent whole/fraction negation bug. Evir needs both
    /// parts for projectile motion; callers that only need pixels use the helpers above.
    /// </summary>
    internal static SignedSixteenSixteen ReadEightBitSineFixedProduct(
        TrigonometryAngle angle,
        int radius)
    {
        // A half-wave byte times a radius byte is below 65,536, so the product needs no wrap.
        int sample = Trigonometry.EightBitHalfWave(angle.HalfWaveIndex);
        SignedSixteenSixteen product = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(
            sample * RadiusMultiplierOperand(radius));
        // The game negates each word independently instead of carrying the
        // fractional increment into the whole word. Preserve that documented quirk.
        return angle.IsInSecondHalf ? product.NegatedPartsSeparately() : product;
    }

    /// <summary>Rotates the fixed sine multiplication by a quarter turn for cosine, retaining its negative-fraction behavior.</summary>
    internal static SignedSixteenSixteen ReadEightBitCosineFixedProduct(
        TrigonometryAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.TurnedBySteps(TrigonometryAngle.QuarterTurn.RawValue), radius);

    /// <summary>Rotates the fixed sine multiplication by a half turn to obtain negative sine.</summary>
    internal static SignedSixteenSixteen ReadEightBitNegativeSineFixedProduct(
        TrigonometryAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.TurnedBySteps(TrigonometryAngle.HalfTurn.RawValue), radius);

    /// <summary>
    /// Computes one half of the shared sine/cosine velocity product.
    /// It reads an unsigned quarter-circle sample of 0 through 65535, multiplies it
    /// by an unsigned magnitude of 0 through 65535, and returns the complete 16.16 product. Callers
    /// supply either a quarter turn (absolute cosine) or a half turn (absolute sine) as the angle offset.
    /// Keeping sample generation deterministic avoids host floating-point rounding and makes the raw
    /// velocities directly comparable with the game's multiplication result.
    /// </summary>
    internal static int ReadUnsignedSineMagnitudeProduct(
        TrigonometryAngle angle,
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
    internal static int ReadAbsoluteCosineProduct(TrigonometryAngle angle, int magnitude) =>
        ReadUnsignedSineMagnitudeProduct(angle, magnitude, TrigonometryAngle.QuarterTurn.RawValue);

    /// <summary>The absolute sine half of the angle-to-vector routine: the sample a half turn ahead.</summary>
    internal static int ReadAbsoluteSineProduct(TrigonometryAngle angle, int magnitude) =>
        ReadUnsignedSineMagnitudeProduct(angle, magnitude, TrigonometryAngle.HalfTurn.RawValue);

    /// <summary>
    /// The horizontal part of a speed of <paramref name="magnitude"/> 256ths of a pixel along <paramref name="angle"/>,
    /// regardless of direction: the absolute cosine product as an unsigned 16.16 speed.
    /// </summary>
    internal static UnsignedSixteenSixteen AbsoluteCosineSpeed(TrigonometryAngle angle, int magnitude) =>
        UnsignedSixteenSixteen.FromRaw(unchecked((uint)ReadAbsoluteCosineProduct(angle, magnitude)));

    /// <summary>
    /// The vertical part of a speed of <paramref name="magnitude"/> 256ths of a pixel along <paramref name="angle"/>,
    /// regardless of direction: the absolute sine product as an unsigned 16.16 speed.
    /// </summary>
    internal static UnsignedSixteenSixteen AbsoluteSineSpeed(TrigonometryAngle angle, int magnitude) =>
        UnsignedSixteenSixteen.FromRaw(unchecked((uint)ReadAbsoluteSineProduct(angle, magnitude)));

    /// <summary>Whether a table angle's cosine component is negative: the tests bit 7 of the angle plus a quarter turn.</summary>
    internal static bool CosineIsNegative(TrigonometryAngle angle) => angle.TurnedBySteps(TrigonometryAngle.QuarterTurn.RawValue).RawValue >= TrigonometryAngle.HalfTurn.RawValue;

    /// <summary>Whether a table angle's sine component is negative: the tests bit 7 of the angle plus a half turn.</summary>
    internal static bool SineIsNegative(TrigonometryAngle angle) => angle.TurnedBySteps(TrigonometryAngle.HalfTurn.RawValue).RawValue >= TrigonometryAngle.HalfTurn.RawValue;

    /// <summary>Multiplies a signed sine-table sample by speed with the game's fixed-point rounding.</summary>
    internal static SignedEightEight MultiplySineSample(int speed, TrigonometryAngle angle) =>
        Trigonometry.MultiplySignedSine(speed, angle);

    /// <summary>The span of a short multiplication's operand or result: values past it continue from zero.</summary>
    private const int OperandSpan = 256;

    /// <summary>
    /// The multiplier operand of 0 through 255 a radius, zero or more, supplies: the short sine multiplication
    /// takes only its place within 256, so a radius of 256 or more continues from zero.
    /// </summary>
    private static int RadiusMultiplierOperand(int radius) => Ensure.AtLeastZero(radius) % OperandSpan;
}
