namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which explosion a Fireflea dies with after Samus touches it (#1592). Every touch kills a Fireflea, even one the
/// touch left alive, and the explosion that death starts is not chosen by the Fireflea: one that survived the touch
/// explodes with the explosion its remaining health names in the explosions' authored order (small, Samus contact,
/// normal, Mini-Kraid's, big), so a Fireflea with one to four health left dies in the contact, normal, Mini-Kraid or
/// big explosion and one with more in the small one; one the touch already killed dies a second time in the small
/// explosion. The rule returns the explosion itself (#1620).
/// </summary>
/// <remarks>
/// Consulted once per touch, after the ordinary touch reaction, as the Fireflea's death starts. Evidence: the Fireflea's
/// touch reaction (<c>$A3:8E6B</c>) calls the enemy death routine straight after the common touch reaction, with the
/// accumulator still holding the health that reaction last loaded, or zero (or the identity of the respawn placeholder
/// the cleared enemy became, a number past every explosion) when the reaction already ran the death and cleared the enemy; the death routine takes zero through
/// four as its explosions in their authored order and any other number as the first.
/// </remarks>
[CompatibilityService("a Fireflea Samus touches dies in the explosion its remaining health names")]
public static class FirefleaTouchDeathExplosion
{
    /// <summary>
    /// The explosion a Fireflea dies with when Samus's touch left it <paramref name="remainingHealth"/> health: the one
    /// at that place in <paramref name="authoredOrder"/>, or the first, the small explosion, past the last.
    /// </summary>
    /// <typeparam name="TExplosion">An enemy's death explosion.</typeparam>
    /// <param name="remainingHealth">The Fireflea's health after the touch reaction, zero or more.</param>
    /// <param name="authoredOrder">The death explosions in their authored order, the small explosion first.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="remainingHealth"/> is negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="authoredOrder"/> is empty.</exception>
    public static TExplosion AfterSurvivedTouch<TExplosion>(int remainingHealth, IReadOnlyList<TExplosion> authoredOrder)
    {
        Ensure.AtLeastZero(remainingHealth);
        return remainingHealth < RequireExplosions(authoredOrder).Count ? authoredOrder[remainingHealth] : authoredOrder[0];
    }

    /// <summary>The explosion a Fireflea the touch already killed dies with a second time: the small explosion, the first.</summary>
    /// <typeparam name="TExplosion">An enemy's death explosion.</typeparam>
    /// <param name="authoredOrder">The death explosions in their authored order, the small explosion first.</param>
    /// <exception cref="ArgumentException"><paramref name="authoredOrder"/> is empty.</exception>
    public static TExplosion AfterKillingTouch<TExplosion>(IReadOnlyList<TExplosion> authoredOrder) =>
        RequireExplosions(authoredOrder)[0];

    /// <summary>Rejects an empty set of explosions.</summary>
    private static IReadOnlyList<TExplosion> RequireExplosions<TExplosion>(IReadOnlyList<TExplosion> authoredOrder) =>
        Ensure.NotNull(authoredOrder).Count > 0
            ? authoredOrder
            : throw new ArgumentException("The death explosions include the small explosion.", nameof(authoredOrder));
}
