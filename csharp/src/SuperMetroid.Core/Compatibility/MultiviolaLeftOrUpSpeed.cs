namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Multiviola turns its speed along an axis leftward or upward (#1620, was
/// <c>MultiviolaBehavior.NegateWithoutFractionOverflow</c>): the speed's whole pixels are complemented and its fraction
/// negated on its own (<see cref="PartwiseReversal.WholeComplementedFractionNegated"/>), which is exact for a speed with a
/// fraction and a pixel an update further left or up for one without. Along an axis its heading gives no speed at all the
/// Multiviola therefore still moves a pixel an update left or up: heading straight right it drifts up a pixel an update,
/// heading straight up it drifts left a pixel an update. The Multiviola placed heading straight left in Norfair's Mickey
/// Mouse room heads straight right, drifting up, after its first wall and floor bounces.
/// </summary>
/// <remarks>
/// Consulted on every update of a Multiviola, once for each axis its heading points left or up along, after its speeds
/// are rebuilt from its heading and before that axis's move. Evidence: the main update (<c>$A2:B443</c> for its horizontal
/// speed, <c>$A2:B47D</c> for its vertical speed) only complements the whole word and negates the fraction word, never
/// carrying the fraction's increment into the whole pixels; the Multiviola left-or-up speed check runs the production
/// update at every heading and every authored speed and compares both moves with that arithmetic.
/// </remarks>
[CompatibilityService("a Multiviola turning a speed left or up complements its whole pixels, so an axis without speed still moves it a pixel an update left or up")]
internal static class MultiviolaLeftOrUpSpeed
{
    /// <summary>The speed a Multiviola moves at along an axis it heads left or up along, at <paramref name="magnitude"/>.</summary>
    /// <param name="magnitude">The speed along the axis regardless of direction, as its heading's sine or cosine gives it.</param>
    internal static SignedSixteenSixteen Of(SignedSixteenSixteen magnitude) =>
        PartwiseReversal.WholeComplementedFractionNegated(magnitude);
}
