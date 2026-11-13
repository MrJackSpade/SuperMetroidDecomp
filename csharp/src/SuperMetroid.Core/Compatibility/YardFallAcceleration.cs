namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Yard in the air gains downward speed (#1620, was <c>YardEnemyBehavior</c>'s "stores the whole pixels only
/// below four"): each update its vertical move meets nothing, an eighth of a pixel an update is added to its vertical
/// speed, but an addition that would leave the speed's whole pixels at four or more down keeps the whole pixels it had
/// and takes only the new fraction. A falling Yard therefore never reaches four pixels an update: from three and seven
/// eighths its fraction wraps to nothing and it falls at three pixels again, so its speed climbs through three to
/// three and seven eighths and starts over every eight updates. A Yard sent down at four pixels an update or more (a
/// fast kick's speed reversed by a ceiling) keeps those whole pixels until it strikes something, its fraction cycling.
/// </summary>
/// <remarks>
/// Consulted on every update of a dropped, kicked or shot Yard's flight whose vertical move meets nothing, after that
/// move. Evidence: the airborne movement (<c>$A3:D1B3</c>) adds the eighth to the fraction word and the fraction's carry
/// to the whole word (<c>$A3:D245</c>-<c>$A3:D255</c>), always stores the fraction and stores the whole word only when
/// the sum less four is negative (<c>CMP #$0004</c>, <c>BPL</c> at <c>$A3:D258</c>). A dropped Yard falls from rest and
/// reaches three pixels an update after 24 updates, about 35 pixels of fall; a kick launches it at three to eight
/// pixels an update upward, which a ceiling reverses; so retail falls reach the held whole pixels and the cycling
/// fraction. The Yard fall acceleration check compares every whole word with each fraction on both sides of the carry,
/// and every fraction with whole words either side of the cap, of no speed and at both ends, with that addition.
/// </remarks>
[CompatibilityService("a Yard in the air gains an eighth of a pixel an update of downward speed, but an addition that would reach four pixels or more down keeps the whole pixels it had while the fraction counts on, so its fall speed cycles below four pixels")]
internal static class YardFallAcceleration
{
    /// <summary>The downward speed each update adds: an eighth of a pixel an update.</summary>
    private static readonly SignedSixteenSixteen Gravity = SignedSixteenSixteen.FromSixteenthsOfPixel(2);

    /// <summary>The whole pixels an update of downward speed an addition may not reach.</summary>
    private const int HeldWholePixels = 4;

    /// <summary>The vertical speed a Yard in the air flies at after one update's gain on <paramref name="speed"/>.</summary>
    /// <param name="speed">The vertical speed, positive down, the Yard flies at before the update's gain.</param>
    internal static SignedSixteenSixteen Accelerated(SignedSixteenSixteen speed)
    {
        SignedSixteenSixteen accelerated = speed + Gravity;
        return CoordinateAxis.Difference(accelerated.UnsignedWholePixels, HeldWholePixels) < 0
            ? accelerated
            : speed.WithFraction(accelerated.Fraction);
    }
}
