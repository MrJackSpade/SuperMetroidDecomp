namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which explosion a Powamp shot dead explodes with, decided by where its spike burst landed in the enemy projectile pool
/// (#1513). A Powamp killed by a shot fires its eight spikes and then dies; the explosion it dies with is not its own but
/// follows from the pool place of the last spike the burst fired: the first place gives the small explosion, the second
/// the normal one, the third the big one, and any later place the small one. A burst that found the pool full before its
/// eighth spike also ends in the small explosion.
/// </summary>
/// <remarks>
/// Consulted once, when the shot that kills a Powamp has fired its spike burst, before the body's death starts. Evidence:
/// <c>FirePowampSpikesIn8Directions</c> leaves the last spawned spike's pool offset (twice its place) in the accumulator,
/// which the shot reaction passes to <c>EnemyDeath</c> as its death animation; <c>EnemyDeath</c> takes zero through four
/// as its explosions in their authored order and any other number as the first. When allocation fails the routine leaves
/// the body's graphics there instead, a number past every explosion for Powamp's only retail variant (#1512).
/// </remarks>
[CompatibilityService("a Powamp shot dead explodes with the explosion its last burst spike's place in the projectile pool selects")]
public static class PowampBurstExplosion
{
    /// <summary>The number of authored death explosions.</summary>
    private const int AuthoredExplosionCount = 5;

    /// <summary>The places, in the explosions' authored order, of the explosions a burst's last spike can select, by its place in the pool.</summary>
    private static readonly int[] AuthoredExplosionByPlace = [0, 2, 4];

    /// <summary>
    /// The explosion a Powamp dies with after a burst whose last spike took pool place <paramref name="lastSpikePlace"/>,
    /// or whose spikes found the pool full when it is null: from <paramref name="authoredOrder"/> (small, Samus contact,
    /// normal, Mini-Kraid's, big), the small, normal or big explosion for the first three places and the small one
    /// otherwise (#1620: the rule returns the explosion itself).
    /// </summary>
    /// <typeparam name="TExplosion">An enemy's death explosion.</typeparam>
    /// <param name="lastSpikePlace">The zero-based pool place of the burst's last spike, or null when the pool was full.</param>
    /// <param name="authoredOrder">The five death explosions in their authored order, the small explosion first.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="authoredOrder"/> does not hold the five explosions.</exception>
    public static TExplosion AfterBurst<TExplosion>(int? lastSpikePlace, IReadOnlyList<TExplosion> authoredOrder)
    {
        if (Ensure.NotNull(authoredOrder).Count != AuthoredExplosionCount)
            throw new ArgumentException("The death explosions are the five authored ones.", nameof(authoredOrder));
        return authoredOrder[lastSpikePlace is int place && Ensure.AtLeastZero(place) < AuthoredExplosionByPlace.Length
            ? AuthoredExplosionByPlace[place]
            : 0];
    }
}