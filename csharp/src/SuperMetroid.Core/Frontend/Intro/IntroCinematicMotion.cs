namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Exact fixed-point additions shared by translated opening-cinematic actors.</summary>
internal static class IntroCinematicMotion
{
    /// <summary>
    /// Adds a fraction of a pixel alone to one axis: <paramref name="fraction"/>, 0 through 65535 sixty-fifth-thousandths
    /// of a pixel, carries into the whole pixel and nothing else is added to it.
    /// </summary>
    public static FixedCoordinate AddFraction(FixedCoordinate position, int fraction) =>
        position.MovedBy(SignedSixteenSixteen.FromParts(0, fraction));

    /// <summary>Adds a signed 16.16 delta, in 1/65536 pixel, to one axis.</summary>
    public static FixedCoordinate AddSignedSixteenSixteen(FixedCoordinate position, int fixedDelta) =>
        position.MovedBy(SignedSixteenSixteen.FromRaw(fixedDelta));

    /// <summary>Adds a signed 8.8 velocity to one axis.</summary>
    public static FixedCoordinate AddEightEight(
        FixedCoordinate position,
        SignedEightEight velocity) =>
        // The velocity's fraction lands in the subposition's high byte.
        // Its eight-bit carry then feeds the sign-extended integer byte.
        velocity
            .ToSixteenSixteenDelta()
            .AddTo(position.Whole, position.Fraction);
}
