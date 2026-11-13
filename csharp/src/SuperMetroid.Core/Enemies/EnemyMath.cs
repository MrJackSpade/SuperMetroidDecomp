namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Stateless enemy arithmetic shared by the species routines: trigonometry angles, whole-pixel velocity
/// steps and sine/cosine products by short radii (<see cref="ShortCircleProduct"/>).
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
    /// <paramref name="distance"/> pixels from its target: a quarter of the distance, rounded
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

        // The bearing is counted in 256ths of a turn clockwise from up, so right lies a quarter turn on, down a half
        // turn and left three quarters; each octant turns from its nearer compass direction by the slope's eighth.
        if (absoluteY < absoluteX)
        {
            // The octant is chosen from the full distances; the slope keeps the original's limits, so offsets
            // beyond 255 pixels bend the angle (AimAngleSlope).
            int divided = AimAngleSlope.Of(absoluteY, absoluteX);
            return Bearing.Wrapped(quadrant switch
            {
                0 => EnemyTrigonometryAngleDefinitions.QuarterTurn + (divided >> 3),
                1 => EnemyTrigonometryAngleDefinitions.QuarterTurn - (divided >> 3),
                2 => -EnemyTrigonometryAngleDefinitions.QuarterTurn - (divided >> 3),
                _ => (divided >> 3) - EnemyTrigonometryAngleDefinitions.QuarterTurn,
            });
        }

        int inverseDivided = AimAngleSlope.Of(absoluteX, absoluteY);
        return Bearing.Wrapped(quadrant switch
        {
            0 => EnemyTrigonometryAngleDefinitions.HalfTurn - (inverseDivided >> 3),
            1 => inverseDivided >> 3,
            2 => EnemyTrigonometryAngleDefinitions.HalfTurn + (inverseDivided >> 3),
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
    /// The whole pixels of the short sine product (<see cref="ReadEightBitSineFixedProduct"/>): in the turn's second half
    /// the reversed whole pixels of the first half's product, as <see cref="ShortCircleProduct.Reversed"/> leaves them, so
    /// <c>Math.Sin</c> or a true fixed-point reversal would disagree by one pixel.
    /// </summary>
    internal static int ReadEightBitSineProduct(TrigonometryAngle angle, int radius) =>
        ReadEightBitSineFixedProduct(angle, radius).WholePixels;

    /// <summary>Computes the short cosine multiplication.</summary>
    internal static int ReadEightBitCosineProduct(TrigonometryAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.TurnedBy(TrigonometryAngle.QuarterTurn), radius);

    /// <summary>Computes the short negative sine multiplication.</summary>
    internal static int ReadEightBitNegativeSineProduct(TrigonometryAngle angle, int radius) =>
        ReadEightBitSineProduct(angle.TurnedBy(TrigonometryAngle.HalfTurn), radius);

    /// <summary>
    /// The point on a circle at a trigonometry angle: the cosine product as X and the negative-sine
    /// product as Y, each wrapped to 0 through 65535.
    /// </summary>
    internal static Offset16<RoomPixels> CircleOffset(TrigonometryAngle angle, int radius) => Offset16<RoomPixels>.Wrapped(
        ReadEightBitCosineProduct(angle, radius),
        ReadEightBitNegativeSineProduct(angle, radius));

    /// <summary>
    /// The point at <paramref name="radius"/> pixels along <paramref name="angle"/>'s sine, in whole pixels and fraction:
    /// a half-wave sample of 0 through 255 times the radius the short circle product uses, in 256ths of a pixel, reversed
    /// in the turn's second half as <see cref="ShortCircleProduct"/> reverses it. Evir needs both parts for its spit's
    /// motion; callers that only need pixels use the helpers above.
    /// </summary>
    internal static SignedSixteenSixteen ReadEightBitSineFixedProduct(
        TrigonometryAngle angle,
        int radius)
    {
        // A half-wave sample below 256 times a radius below 256 is below 65,536, so the product needs no wrap.
        int sample = Trigonometry.EightBitHalfWave(angle.HalfWaveIndex);
        SignedSixteenSixteen product = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(
            sample * ShortCircleProduct.RadiusInUse(radius));
        return angle.IsInSecondHalf ? ShortCircleProduct.Reversed(product) : product;
    }

    /// <summary>Rotates the fixed sine multiplication by a quarter turn for cosine, retaining its negative-fraction behavior.</summary>
    internal static SignedSixteenSixteen ReadEightBitCosineFixedProduct(
        TrigonometryAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.TurnedBy(TrigonometryAngle.QuarterTurn), radius);

    /// <summary>Rotates the fixed sine multiplication by a half turn to obtain negative sine.</summary>
    internal static SignedSixteenSixteen ReadEightBitNegativeSineFixedProduct(
        TrigonometryAngle angle,
        int radius) =>
        ReadEightBitSineFixedProduct(angle.TurnedBy(TrigonometryAngle.HalfTurn), radius);

    /// <summary>
    /// The share of a speed of <paramref name="wholePixels"/> whole pixels per update that <paramref name="angle"/>'s
    /// unsigned half-wave sample gives, regardless of direction: the sample, 0 through 65535 in 65536ths of the whole
    /// speed (its peak of 65535 standing for all of it), times the speed, in pixels per update with the fraction in
    /// 65536ths. The speeds are whole pixels: a Draygon goop's is two, a Multiviola's its spawn speed. Keeping sample
    /// generation deterministic avoids host floating-point rounding, so each speed is exactly the game's product.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wholePixels"/> is negative.</exception>
    /// <exception cref="OverflowException">The product does not fit a 16.16 speed.</exception>
    internal static UnsignedSixteenSixteen UnsignedSineSpeed(TrigonometryAngle angle, int wholePixels) =>
        UnsignedSixteenSixteen.ShareOfWholePixels(wholePixels, Trigonometry.UnsignedHalfWave(angle.HalfWaveIndex));

    /// <summary>
    /// The horizontal part of a speed of <paramref name="wholePixels"/> whole pixels per update along
    /// <paramref name="angle"/>, regardless of direction: <see cref="UnsignedSineSpeed"/> a quarter turn ahead.
    /// </summary>
    internal static UnsignedSixteenSixteen AbsoluteCosineSpeed(TrigonometryAngle angle, int wholePixels) =>
        UnsignedSineSpeed(angle.TurnedBy(TrigonometryAngle.QuarterTurn), wholePixels);

    /// <summary>
    /// The vertical part of a speed of <paramref name="wholePixels"/> whole pixels per update along
    /// <paramref name="angle"/>, regardless of direction: <see cref="UnsignedSineSpeed"/> a half turn ahead.
    /// </summary>
    internal static UnsignedSixteenSixteen AbsoluteSineSpeed(TrigonometryAngle angle, int wholePixels) =>
        UnsignedSineSpeed(angle.TurnedBy(TrigonometryAngle.HalfTurn), wholePixels);

    /// <summary>Whether a table angle's cosine component is negative: the angle a quarter turn on lies in the turn's second half.</summary>
    internal static bool CosineIsNegative(TrigonometryAngle angle) => angle.TurnedBy(TrigonometryAngle.QuarterTurn).IsInSecondHalf;

    /// <summary>Whether a table angle's sine component is negative: the angle a half turn on lies in the turn's second half.</summary>
    internal static bool SineIsNegative(TrigonometryAngle angle) => angle.TurnedBy(TrigonometryAngle.HalfTurn).IsInSecondHalf;

    /// <summary>
    /// The velocity of <paramref name="wholePixels"/> whole pixels per update, 0 through 127, along
    /// <paramref name="angle"/>'s sine: the signed sine sample, which peaks at one pixel per update, times the whole
    /// speed. The product of a whole speed loses nothing, so it is the sample scaled exactly; the routines that read the
    /// sample itself as a velocity take one pixel.
    /// </summary>
    internal static SignedEightEight SineVelocity(TrigonometryAngle angle, int wholePixels) =>
        Trigonometry.MultiplySignedSine(SignedEightEight.FromPixels(Ensure.BetweenInclusive(0, sbyte.MaxValue, wholePixels)), angle);

    /// <summary>Multiplies a signed sine-table sample by speed with the game's fixed-point rounding.</summary>
    internal static SignedEightEight MultiplySineSample(SignedEightEight speed, TrigonometryAngle angle) =>
        Trigonometry.MultiplySignedSine(speed, angle);
}
