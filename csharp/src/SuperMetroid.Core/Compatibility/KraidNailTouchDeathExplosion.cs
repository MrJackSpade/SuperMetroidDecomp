namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which explosion a Kraid fingernail dies with after Samus touches it (#1620). Every touch that leaves a fingernail in
/// its place kills it, and the explosion is the one its remaining health names in the explosions' authored order (small,
/// Samus contact, normal, Mini-Kraid's, big): a fingernail with one to four health left dies in the contact, normal,
/// Mini-Kraid or big explosion, and one with none or more in the small one.
/// </summary>
/// <remarks>
/// Consulted once per touch, after the ordinary touch reaction, when the fingernail is still in its place. Evidence: the
/// fingernails' touch reactions call the enemy death routine straight after the common touch reaction, with the health
/// that reaction last loaded still in the accumulator; the death routine takes zero through four as its explosions in
/// their authored order and any other number as the first.
/// </remarks>
[CompatibilityService("a Kraid fingernail Samus touches dies in the explosion its remaining health names")]
public static class KraidNailTouchDeathExplosion
{
    /// <summary>
    /// The explosion a fingernail dies with when Samus's touch left it <paramref name="remainingHealth"/> health: the one
    /// at that place in <paramref name="authoredOrder"/>, or the first, the small explosion, past the last.
    /// </summary>
    /// <typeparam name="TExplosion">An enemy's death explosion.</typeparam>
    /// <param name="remainingHealth">The fingernail's health after the touch reaction, zero or more.</param>
    /// <param name="authoredOrder">The death explosions in their authored order, the small explosion first.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="remainingHealth"/> is negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="authoredOrder"/> is empty.</exception>
    public static TExplosion AfterTouch<TExplosion>(int remainingHealth, IReadOnlyList<TExplosion> authoredOrder)
    {
        Ensure.AtLeastZero(remainingHealth);
        if (Ensure.NotNull(authoredOrder).Count == 0)
            throw new ArgumentException("The death explosions include the small explosion.", nameof(authoredOrder));
        return remainingHealth < authoredOrder.Count ? authoredOrder[remainingHealth] : authoredOrder[0];
    }
}
