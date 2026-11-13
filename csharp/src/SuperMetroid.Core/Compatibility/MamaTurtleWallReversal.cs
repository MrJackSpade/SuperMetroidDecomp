namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a hovering Mama Turtle turns back at a wall (#1620, was <c>MamaTurtleBehavior.ReverseMamaTurtleAtWall</c>'s
/// "lowered" carry): its horizontal velocity and acceleration are each reversed by complementing the whole pixels and
/// negating the fraction on its own, which reverses a value exactly unless its fraction is zero, when the reversed value
/// lies one pixel an update further left; and when the turtle met the wall moving left by less than a pixel an update
/// (its velocity's whole pixels at minus one), the reversed acceleration lies one more pixel further left. The turtle,
/// and Samus riding it, therefore leaves a wall by those pixels faster or slower, and its hover drifts by them.
/// </summary>
/// <remarks>
/// Consulted once per wall hit of the hovering turtle, after its horizontal move reports the wall and before the hit's
/// earthquake and sound. Evidence: the wall branch (<c>$A2:903C</c>) negates the sub-velocity, then adds zero with the
/// carry the blocked move left set to the velocity before negating it, then negates the sub-acceleration and adds zero
/// with the carry that velocity addition left to the acceleration before only complementing it; the velocity addition
/// passes the carry on exactly when the whole pixels are minus one. The Mama Turtle reversal check compares both with
/// that arithmetic for every whole and fraction class.
/// </remarks>
[CompatibilityService("a Mama Turtle reversing at a wall complements its speeds' whole pixels, and lowers its acceleration a pixel more after a hit at under a pixel an update leftward")]
internal static class MamaTurtleWallReversal
{
    /// <summary>The whole pixels of a velocity moving left by less than a pixel an update.</summary>
    private const int UnderAPixelLeftward = -1;

    /// <summary>The velocity a turtle that met the wall at <paramref name="velocity"/> leaves it with.</summary>
    /// <param name="velocity">The horizontal velocity the turtle met the wall with.</param>
    internal static SignedSixteenSixteen ReversedVelocity(SignedSixteenSixteen velocity) =>
        velocity.ComplementedWholeNegatedFraction();

    /// <summary>
    /// The acceleration a turtle that met the wall at <paramref name="velocity"/> with <paramref name="acceleration"/>
    /// leaves it with: reversed as the velocity is, and one pixel further left after a hit at under a pixel an update
    /// leftward.
    /// </summary>
    /// <param name="acceleration">The horizontal acceleration the turtle met the wall with.</param>
    /// <param name="velocity">The horizontal velocity the turtle met the wall with.</param>
    internal static SignedSixteenSixteen ReversedAcceleration(SignedSixteenSixteen acceleration, SignedSixteenSixteen velocity)
    {
        SignedSixteenSixteen reversed = acceleration.ComplementedWholeNegatedFraction();
        return velocity.WholePixels == UnderAPixelLeftward ? reversed - SignedSixteenSixteen.FromWholePixels(1) : reversed;
    }
}
