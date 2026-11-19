namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Stateless enemy arithmetic shared by the species: bearings, whole-pixel velocity clamps, points on circles of short
/// radius (<see cref="ShortCircleProduct"/>) and the parts of a speed along an angle's sine and cosine.
/// </summary>
internal static class EnemyMath
{
    /// <summary>
    /// Holds a velocity within <paramref name="fastest"/>, a whole-pixel speed, either way (#2555): a velocity at or past
    /// it becomes exactly it, one below its reversal exactly the reversal, and any other keeps its fraction.
    /// </summary>
    internal static SignedSixteenSixteen LimitedEitherWay(SignedSixteenSixteen velocity, SignedSixteenSixteen fastest) =>
        velocity >= fastest ? fastest : velocity < -fastest ? -fastest : velocity;

    /// <summary>The share of its distance from its target a homing Metroid or Mochtroid pulls by each update: a 1024th.</summary>
    private const int HomingPullShare = 1024;

    /// <summary>
    /// The proportional pull a Metroid or Mochtroid takes from its velocity for a signed distance of
    /// <paramref name="distance"/> pixels from its target: a 1024th of the distance, a quarter of its pixels in 256ths of a
    /// pixel, rounded toward minus infinity to the 256th (<see cref="SignedSixteenSixteen.DividedInTwoFiftySixths"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The distance lies outside a signed distance's whole pixels.</exception>
    internal static SignedSixteenSixteen HomingPull(int distance) =>
        SignedSixteenSixteen.FromWholePixels(distance).DividedInTwoFiftySixths(HomingPullShare);

    /// <summary>
    /// The direction of an offset, as every enemy aims along one (#2555): zero points right and a quarter turn up, the
    /// angle the game's sine and cosine take.
    /// </summary>
    internal static TrigonometryAngle CalculateAimAngle(Offset16<RoomPixels> delta)
    {
        // The distances are taken as whole numbers, so the farthest offset either way keeps its full size.
        int absoluteX = Math.Abs((int)delta.X);
        int absoluteY = Math.Abs((int)delta.Y);
        bool left = delta.X < 0;
        bool up = delta.Y < 0;

        // Each octant turns from its nearer compass direction toward the other axis's direction by the slope's eighth:
        // clockwise, toward decreasing angles, from right toward down and from left toward up, so from up or down the
        // other way round.
        bool turnsClockwiseFromSide = left == up;
        if (absoluteY < absoluteX)
        {
            // The octant is chosen from the full distances; the slope keeps the game's limits, so offsets
            // beyond 255 pixels bend the angle (AimAngleSlope).
            int turn = AimAngleSlope.TurnSteps(absoluteY, absoluteX);
            return (left ? TrigonometryAngle.HalfTurn : TrigonometryAngle.Zero).TurnedBySteps(turnsClockwiseFromSide ? -turn : turn);
        }

        int inverseTurn = AimAngleSlope.TurnSteps(absoluteX, absoluteY);
        return (up ? TrigonometryAngle.QuarterTurn : TrigonometryAngle.Zero - TrigonometryAngle.QuarterTurn)
            .TurnedBySteps(turnsClockwiseFromSide ? inverseTurn : -inverseTurn);
    }

    /// <summary>
    /// The whole pixels of the sine distance of the point <paramref name="radius"/> pixels along <paramref name="angle"/>
    /// (<see cref="CircleSine"/>): in the turn's second half the reversed whole pixels of the first half's distance, as
    /// <see cref="ShortCircleProduct.Reversed"/> leaves them, so <c>Math.Sin</c> or a reversal of the whole distance would
    /// disagree by one pixel.
    /// </summary>
    internal static int CircleSinePixels(TrigonometryAngle angle, int radius) =>
        CircleSine(angle, radius).WholePixels;

    /// <summary>The whole pixels of the point's cosine distance: <see cref="CircleSinePixels"/> a quarter turn ahead.</summary>
    internal static int CircleCosinePixels(TrigonometryAngle angle, int radius) =>
        CircleSinePixels(angle.TurnedBy(TrigonometryAngle.QuarterTurn), radius);

    /// <summary>The whole pixels of the point's negative sine distance: <see cref="CircleSinePixels"/> a half turn ahead.</summary>
    internal static int CircleNegativeSinePixels(TrigonometryAngle angle, int radius) =>
        CircleSinePixels(angle.TurnedBy(TrigonometryAngle.HalfTurn), radius);

    /// <summary>
    /// The point on a circle at a trigonometry angle: the cosine distance in pixels as X and the negative sine distance
    /// as Y, each no farther from the centre than the radius, a few blocks at most for every circling enemy.
    /// </summary>
    internal static Offset16<RoomPixels> CircleOffset(TrigonometryAngle angle, int radius) => Offset16<RoomPixels>.Of(
        CircleCosinePixels(angle, radius),
        CircleNegativeSinePixels(angle, radius));

    /// <summary>
    /// The sine distance of the point <paramref name="radius"/> pixels along <paramref name="angle"/>, in whole pixels and
    /// fraction (#2537): the exact reach (<see cref="UnitComponent.ExactReachOf"/>) of the coarse half-wave's share
    /// (<see cref="UnitComponent.CoarseHalfWaveOf"/>) along the radius the short circle product uses, reversed in the
    /// turn's second half as <see cref="ShortCircleProduct"/> reverses it. Evir needs both parts for its spit's motion; an
    /// enemy that only needs pixels uses the sine distances above.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="radius"/> is negative.</exception>
    internal static SignedSixteenSixteen CircleSine(
        TrigonometryAngle angle,
        int radius)
    {
        // The share is under one and the radius in use under 256 pixels, so the reach is under 256 pixels.
        SignedSixteenSixteen reach = UnitComponent.CoarseHalfWaveOf(angle).ExactReachOf(ShortCircleProduct.RadiusInUse(radius));
        return angle.IsInSecondHalf ? ShortCircleProduct.Reversed(reach) : reach;
    }

    /// <summary>The point's cosine distance: <see cref="CircleSine"/> a quarter turn ahead, keeping its second-half reversal.</summary>
    internal static SignedSixteenSixteen CircleCosine(
        TrigonometryAngle angle,
        int radius) =>
        CircleSine(angle.TurnedBy(TrigonometryAngle.QuarterTurn), radius);

    /// <summary>The point's negative sine distance: <see cref="CircleSine"/> a half turn ahead.</summary>
    internal static SignedSixteenSixteen CircleNegativeSine(
        TrigonometryAngle angle,
        int radius) =>
        CircleSine(angle.TurnedBy(TrigonometryAngle.HalfTurn), radius);

    /// <summary>
    /// The share of a speed of <paramref name="wholePixels"/> whole pixels per update that <paramref name="angle"/>'s
    /// unsigned half-wave gives, regardless of direction: the half-wave's share of the whole speed, its peak standing for
    /// all of it, times the speed, in pixels per update to a fraction of a pixel. The speeds are whole pixels: a Draygon goop's is two, a Multiviola's its spawn speed. Computing the
    /// half-wave deterministically avoids floating-point rounding, so each speed is exactly the game's product.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wholePixels"/> is negative.</exception>
    /// <exception cref="OverflowException">The product is 65536 pixels or more.</exception>
    private static UnsignedSixteenSixteen UnsignedSineSpeed(TrigonometryAngle angle, int wholePixels) =>
        UnsignedSixteenSixteen.ShareOfWholePixels(wholePixels, Trigonometry.UnsignedHalfWave(angle.StepsIntoHalfTurn));

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

    /// <summary>Whether an angle's cosine component is negative: the angle a quarter turn on lies in the turn's second half.</summary>
    internal static bool CosineIsNegative(TrigonometryAngle angle) => angle.TurnedBy(TrigonometryAngle.QuarterTurn).IsInSecondHalf;

    /// <summary>Whether an angle's sine component is negative: the angle a half turn on lies in the turn's second half.</summary>
    internal static bool SineIsNegative(TrigonometryAngle angle) => angle.TurnedBy(TrigonometryAngle.HalfTurn).IsInSecondHalf;

    /// <summary>
    /// The velocity of <paramref name="wholePixels"/> whole pixels per update along <paramref name="angle"/> (#2539), angle
    /// zero pointing right and a quarter turn on pointing up: the speed along each axis of the angle's share across and
    /// down (<see cref="FineUnitComponent"/>). Each component is the share of the speed rounded down and then given its
    /// direction, so it rounds toward zero: a component pointing left or up is exactly the reverse of the same component
    /// pointing right or down.
    /// </summary>
    /// <param name="angle">The direction of the velocity.</param>
    /// <param name="wholePixels">The speed along the direction in whole pixels per update, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wholePixels"/> is negative.</exception>
    internal static FixedVector<RoomPixels> VelocityAlong(TrigonometryAngle angle, int wholePixels) => new(
        FineUnitComponent.AcrossOf(angle).SpeedAt(wholePixels),
        FineUnitComponent.DownOf(angle).SpeedAt(wholePixels));

    /// <summary>
    /// The velocity of <paramref name="wholePixels"/> whole pixels per update, none or more, along <paramref name="angle"/>'s
    /// sine: the signed sine, which peaks at one pixel per update, times the whole speed. The product of a whole speed
    /// loses nothing, so it is the sine scaled exactly; the behaviours that read the sine itself as a velocity take one
    /// pixel.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wholePixels"/> is negative.</exception>
    internal static SignedEightEight SineVelocity(TrigonometryAngle angle, int wholePixels) =>
        SpeedAlongAngle.Along(SignedEightEight.FromPixels(wholePixels >= 0
            ? wholePixels
            : throw new ArgumentOutOfRangeException(nameof(wholePixels), wholePixels, "A sine velocity's speed is none or more.")), angle);

    /// <summary>
    /// The part of <paramref name="speed"/> along <paramref name="angle"/>'s sine, rounding toward zero as the game does
    /// (<see cref="SpeedAlongAngle.Along"/>); a quarter turn ahead gives the cosine part.
    /// </summary>
    internal static SignedEightEight SineSpeed(SignedEightEight speed, TrigonometryAngle angle) =>
        SpeedAlongAngle.Along(speed, angle);
}
