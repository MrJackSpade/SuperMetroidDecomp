namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The two ways the original reverses some enemies' speeds one part at a time, the fraction's carry never reaching the
/// whole pixels (#1620, were <c>SignedSixteenSixteen.NegatedPartsSeparately</c> and
/// <c>SignedSixteenSixteen.ComplementedWholeNegatedFraction</c>): negating the whole pixels and the fraction each on their
/// own, which reverses a speed with no fraction exactly and leaves one with a fraction a pixel an update short of its true
/// reversal, back along its first direction; and complementing the whole pixels while negating the fraction, which
/// reverses a speed with a fraction exactly and leaves one with none a pixel an update past its true reversal, so a
/// speed of nothing becomes a pixel an update backward.
/// </summary>
/// <remarks>
/// Consulted only by the services that name each reversal for the game, at the moment each enemy reverses its speed:
/// <see cref="ShortCircleProduct"/>, <see cref="MamaTurtleWallReversal"/>, <see cref="MultiviolaLeftOrUpSpeed"/> and
/// <see cref="YardAirborneReversal"/>. Evidence: those enemies' routines negate the fraction word on its own and then
/// negate or only complement the whole word, whose result never depends on the fraction (the short circle product, the
/// Mama Turtle's wall branch <c>$A2:903C</c>, which adds the carry its blocked move leaves set, the Multiviola's main update
/// <c>$A2:B443</c> and <c>$A2:B47D</c>, the Yard's airborne bounces <c>$A3:D20A</c> and <c>$A3:D26D</c> and its kick
/// <c>$A3:D4F6</c>); the Yard airborne reversal check compares the first form, and the Mama Turtle wall reversal and
/// Multiviola left-or-up speed checks the second, with that arithmetic.
/// </remarks>
[CompatibilityService("a speed reversed one part at a time: its whole pixels negated or complemented and its fraction negated on its own, the fraction's carry never reaching the whole pixels")]
internal static class PartwiseReversal
{
    /// <summary>
    /// <paramref name="speed"/> reversed by negating its whole pixels and its fraction each on their own: its true reversal
    /// when it has no fraction, a pixel an update back along its first direction when it has one.
    /// </summary>
    /// <param name="speed">The speed reversed.</param>
    internal static SignedSixteenSixteen BothPartsNegated(SignedSixteenSixteen speed) =>
        SignedSixteenSixteen.FromParts(unchecked((short)-speed.WholePixels), unchecked((ushort)-speed.Fraction));

    /// <summary>
    /// <paramref name="speed"/> reversed by complementing its whole pixels and negating its fraction on its own: its true
    /// reversal when it has a fraction, a pixel an update further along the reversed direction when it has none.
    /// </summary>
    /// <param name="speed">The speed reversed.</param>
    internal static SignedSixteenSixteen WholeComplementedFractionNegated(SignedSixteenSixteen speed) =>
        SignedSixteenSixteen.FromParts(~speed.WholePixels, unchecked((ushort)-speed.Fraction));
}
