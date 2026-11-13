namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Yard in the air reverses a speed (#1620, was <c>YardEnemyBehavior.NegatePartsSeparately</c>): when Samus facing
/// left kicks it, when it bounces off a wall, and when it bounces off a ceiling or strikes the floor at three pixels an
/// update or more, the speed's whole pixels and its fraction are each negated on their own
/// (<see cref="PartwiseReversal.BothPartsNegated"/>). A speed with no fraction is reversed exactly; one with a fraction
/// leaves a pixel an update short of its true reversal, back along its first direction, so a kicked shell flies left a
/// pixel an update slower than Samus's speed, and a bouncing one rebounds a pixel an update slower than it struck.
/// </summary>
/// <remarks>
/// Consulted when a kick by Samus facing left sets the Yard's speeds, and on each update of its flight after its
/// horizontal move reports a wall or its vertical move reports a ceiling or a fast floor strike. Evidence: the kick
/// (<c>$A3:D4F6</c>) and the airborne movement's wall (<c>$A3:D20A</c>) and vertical (<c>$A3:D26D</c>) bounces negate the
/// fraction word and the whole word each on their own, never carrying the fraction's increment into the whole pixels;
/// the kick takes Samus's horizontal speed with its fraction, and the gravity and slowing steps add fractions to the
/// speeds, so retail flights reach both forms. The Yard airborne reversal check compares every whole word and every
/// fraction with that arithmetic.
/// </remarks>
[CompatibilityService("a Yard in the air reverses a speed by negating its whole pixels and fraction each on their own, leaving a speed with a fraction a pixel an update short of its reversal")]
internal static class YardAirborneReversal
{
    /// <summary>The speed a Yard in the air leaves with after reversing <paramref name="speed"/>.</summary>
    /// <param name="speed">The speed the Yard reverses.</param>
    internal static SignedSixteenSixteen Reversed(SignedSixteenSixteen speed) => PartwiseReversal.BothPartsNegated(speed);
}
