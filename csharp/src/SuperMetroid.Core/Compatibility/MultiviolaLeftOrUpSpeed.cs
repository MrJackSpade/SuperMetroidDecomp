namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Multiviola turns its speed along an axis leftward or upward: the speed's whole pixels are complemented and its
/// fraction negated on its own (<see cref="PartwiseReversal.WholeComplementedFractionNegated"/>), which is exact for a
/// speed with a fraction and a pixel an update further left or up for one without. Along an axis its heading gives no
/// speed at all the Multiviola therefore still moves a pixel an update left or up: heading straight right it drifts up
/// a pixel an update, heading straight up it drifts left a pixel an update. The Multiviola placed heading straight left
/// in Norfair's Mickey Mouse room heads straight right, drifting up, after its first wall and floor bounces.
/// </summary>
/// <remarks>
/// Consulted on every update of a Multiviola, as its velocity is rebuilt from its heading and before its moves. Its heading
/// points it the other way from the angle the game's sine and cosine take: each axis's speed is the heading's unsigned
/// half-wave share of its whole speed, taken right or down where the heading a quarter turn on (across) or a half turn on
/// (down) lies in the turn's second half, and left or up where it lies in the first. Evidence: the main update (<c>$A2:B443</c> for its horizontal
/// speed, <c>$A2:B47D</c> for its vertical speed) only complements the whole word and negates the fraction word, never
/// carrying the fraction's increment into the whole pixels; the Multiviola left-or-up speed check runs the production
/// update at every heading and every authored speed and compares both moves with that arithmetic.
/// </remarks>
[CompatibilityService("a Multiviola turning a speed left or up complements its whole pixels, so an axis without speed still moves it a pixel an update left or up")]
internal static partial class MultiviolaLeftOrUpSpeed
{
    /// <summary>
    /// The velocity a Multiviola heading <paramref name="heading"/> at <paramref name="wholePixels"/> whole pixels an update
    /// moves at (#2569): on each axis the heading's share of the speed toward the heading, an axis it heads left or up
    /// along taking the left-or-up speed.
    /// </summary>
    /// <param name="heading">The Multiviola's heading.</param>
    /// <param name="wholePixels">Its spawn speed in whole pixels an update, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wholePixels"/> is negative.</exception>
    internal static FixedVector<RoomPixels> VelocityAlong(TrigonometryAngle heading, int wholePixels) => new(
        AlongAxis(heading.TurnedBy(TrigonometryAngle.QuarterTurn), wholePixels),
        AlongAxis(heading.TurnedBy(TrigonometryAngle.HalfTurn), wholePixels));

    /// <summary>
    /// The speed along one axis whose share of <paramref name="wholePixels"/> the half-wave at <paramref name="axisAngle"/>
    /// gives: right or down where the angle lies in the turn's second half, and turned left or up with its whole pixels
    /// complemented and its fraction negated where it lies in the first.
    /// </summary>
    private static SignedSixteenSixteen AlongAxis(TrigonometryAngle axisAngle, int wholePixels)
    {
        SignedSixteenSixteen share = UnsignedSixteenSixteen.ShareOfWholePixels(
            wholePixels, Trigonometry.UnsignedHalfWave(axisAngle.StepsIntoHalfTurn)).AsDisplacement();
        return axisAngle.IsInSecondHalf ? share : PartwiseReversal.WholeComplementedFractionNegated(share);
    }
}
