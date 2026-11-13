namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Yard in the air slows its sideways speed (#1620, was <c>YardEnemyBehavior</c>'s "declines to replace the whole
/// part" quirk): each update its flight meets no wall, a sixteenth of a pixel an update is taken toward the left from a
/// speed of nothing or more and toward the right from a leftward one, but a slowing that would leave the speed's whole
/// pixels at nothing keeps the whole pixels it had and takes only the new fraction. A shell flying right therefore never
/// slows below a pixel an update: from just under two it falls to one, then jumps back to just under two; one flying left
/// never slows below a sixteenth of a pixel an update, and one that reaches a leftward sixteenth jumps to a whole pixel an
/// update left. A shell kicked while the camera stands still drifts left at up to a pixel an update.
/// </summary>
/// <remarks>
/// Consulted on every update of a kicked or shot Yard's flight whose sideways move meets no wall, after that move and
/// before its vertical move; a dropped Yard has no sideways flight. Evidence: the airborne movement (<c>$A3:D1B3</c>) adds
/// the slowing to the fraction word and its sign extension, with the fraction's carry, to the whole word
/// (<c>$A3:D1F1</c>-<c>$A3:D200</c>), always stores the fraction and stores the whole word only when the sum is not zero
/// (<c>BEQ</c> at <c>$A3:D202</c>); the kick (<c>$A3:D49F</c>) gives the flight the camera's sideways speed with its
/// fraction and the shot (<c>$A3:D557</c>) a whole pixel an update the way Samus faces, so retail flights slow through
/// both forms. The Yard airborne slowing check
/// compares every whole word with each fraction on both sides of both carries, and every fraction with whole words either
/// side of no speed and at both ends, with that addition.
/// </remarks>
[CompatibilityService("a Yard in the air slows by a sixteenth of a pixel an update, but a slowing that would leave its whole pixels at nothing keeps the whole pixels it had, so its flight never slows to a stop")]
internal static class YardAirborneSlowing
{
    /// <summary>The slowing each update takes from the Yard's sideways speed: a sixteenth of a pixel an update.</summary>
    private static readonly SignedSixteenSixteen Slowing = SignedSixteenSixteen.FromSixteenthsOfPixel(1);

    /// <summary>The sideways speed a Yard in the air flies at after one update's slowing of <paramref name="speed"/>.</summary>
    /// <param name="speed">The sideways speed the Yard flies at before the update's slowing.</param>
    internal static SignedSixteenSixteen Slowed(SignedSixteenSixteen speed)
    {
        SignedSixteenSixteen slowed = speed.IsNegative ? speed + Slowing : speed - Slowing;
        return slowed.WholePixels != 0 ? slowed : speed.WithFraction(slowed.Fraction);
    }
}
