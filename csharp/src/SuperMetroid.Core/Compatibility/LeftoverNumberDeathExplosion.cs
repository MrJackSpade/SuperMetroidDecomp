namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which explosion an enemy dies in when its death takes the explosion from a number left over by what ran before it,
/// rather than one the enemy chose (#2131). Zero through four name the death explosions in their authored order (small,
/// Samus contact, normal, Mini-Kraid's, big), and any other number, or a leftover that is no number at all, names the
/// first, the small explosion. Three deaths take their explosion so: a Fireflea Samus touches and a Kraid fingernail
/// Samus's touch leaves in its place die in the explosion their health left by the touch names, so one with one to four
/// health left dies in the contact, normal, Mini-Kraid or big explosion and one with none or more in the small one; and a
/// Powamp shot dead dies in the explosion twice its burst's last spike's place in the enemy projectile pool names, so the
/// first three places give the small, normal and big explosions and any later place, or a burst into a full pool, the
/// small one. Each caller names where its leftover comes from.
/// </summary>
/// <remarks>
/// Consulted once per such death, as it starts: after the ordinary touch reaction of a Fireflea or a fingernail, and when
/// the shot that kills a Powamp has fired its spike burst. Evidence: the Fireflea's and the fingernails' touch reactions
/// (<c>$A3:8E6B</c> among them) call the enemy death routine straight after the common touch reaction, with the
/// accumulator still holding the health that reaction last loaded, or zero, or the identity of the respawn placeholder
/// the cleared enemy became, once the reaction already ran the death; <c>FirePowampSpikesIn8Directions</c> leaves the last
/// spawned spike's pool offset, twice its place, in the accumulator, which the shot reaction passes on, and leaves the
/// body's graphics there, a number past every explosion for Powamp's only retail variant, when allocation fails
/// (#1512); the death routine takes zero through four as its explosions in their authored order and any other number as
/// the first.
/// </remarks>
[CompatibilityService("an enemy death that takes its explosion from a leftover number dies in the explosion the number names in the authored order, or in the small explosion past the last")]
public static class LeftoverNumberDeathExplosion
{
    /// <summary>The number of authored death explosions, which the leftover numbers zero through four name.</summary>
    private const int AuthoredExplosionCount = 5;

    /// <summary>
    /// The explosion <paramref name="leftoverNumber"/> names: the one at that place in <paramref name="authoredOrder"/>,
    /// or the first, the small explosion, past the last or when the leftover is no number.
    /// </summary>
    /// <typeparam name="TExplosion">An enemy's death explosion.</typeparam>
    /// <param name="leftoverNumber">The number left over for the death, zero or more, or null when what is left is no number.</param>
    /// <param name="authoredOrder">The five death explosions in their authored order, the small explosion first.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="leftoverNumber"/> is negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="authoredOrder"/> does not hold the five explosions.</exception>
    public static TExplosion Named<TExplosion>(int? leftoverNumber, IReadOnlyList<TExplosion> authoredOrder)
    {
        if (Ensure.NotNull(authoredOrder).Count != AuthoredExplosionCount)
            throw new ArgumentException("The death explosions are the five authored ones.", nameof(authoredOrder));
        return leftoverNumber is int number && Ensure.AtLeastZero(number) < AuthoredExplosionCount
            ? authoredOrder[number]
            : authoredOrder[0];
    }
}
