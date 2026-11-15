namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Exact fixed-point additions shared by opening-cinematic actors, on signed screen places (#2000).</summary>
internal static class IntroCinematicMotion
{
    /// <summary>
    /// Adds only the fraction of <paramref name="delta"/> to one axis: the fraction carries into the whole pixel and
    /// nothing else is added to it, so a leftward delta, whose fraction is the rest of a pixel, moves the axis right.
    /// </summary>
    public static SignedSixteenSixteen AddFraction(SignedSixteenSixteen position, SignedSixteenSixteen delta) =>
        position + SignedSixteenSixteen.FromParts(0, delta.Fraction);

    /// <summary>
    /// Adds a signed 1/256-pixel velocity to one axis: the velocity's fraction lands in the upper half of the axis's
    /// fraction, and its overflow past a whole pixel carries into the whole pixels.
    /// </summary>
    public static SignedSixteenSixteen AddEightEight(SignedSixteenSixteen position, SignedEightEight velocity) =>
        position + velocity.ToSixteenSixteenDelta();
}
