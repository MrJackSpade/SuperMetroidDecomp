namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Exact fixed-point additions shared by opening-cinematic actors.</summary>
internal static class IntroCinematicMotion
{
    /// <summary>
    /// Adds only the fraction of <paramref name="delta"/> to one axis: the fraction carries into the whole pixel and
    /// nothing else is added to it, so a leftward delta, whose fraction is the rest of a pixel, moves the axis right.
    /// </summary>
    public static FixedCoordinate AddFraction(FixedCoordinate position, SignedSixteenSixteen delta) =>
        position.MovedBy(SignedSixteenSixteen.FromParts(0, delta.Fraction));

    /// <summary>Adds a signed 8.8 velocity to one axis.</summary>
    public static FixedCoordinate AddEightEight(
        FixedCoordinate position,
        SignedEightEight velocity) =>
        // The velocity's fraction lands in the subposition's upper half.
        // Its overflow past 255 then feeds the signed whole part.
        velocity
            .ToSixteenSixteenDelta()
            .AddTo(position.Whole, position.Fraction);
}
