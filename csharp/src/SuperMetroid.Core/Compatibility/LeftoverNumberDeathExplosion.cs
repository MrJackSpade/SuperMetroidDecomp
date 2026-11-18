namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which explosion an enemy dies in when its death takes the explosion from a number left over by what ran before it,
/// rather than one the enemy chose (#2131). The service owns the order the numbers count in (#2455): zero names the small
/// explosion, one Samus's contact's, two the normal one, three Fake Kraid's and four the big one, and any other number, or
/// a leftover that is no number at all, names the small explosion. The enemies give it their five explosions by name. Three deaths take their explosion so: a Fireflea Samus touches and a Kraid fingernail
/// Samus's touch leaves in its place die in the explosion their health left by the touch names, so one with one to four
/// health left dies in the contact, normal, Fake Kraid or big explosion and one with none or more in the small one; and a
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
/// <typeparam name="TExplosion">An enemy's death explosion.</typeparam>
/// <param name="small">The small explosion, which zero, any number past four and no number name.</param>
/// <param name="samusContact">The explosion of an enemy Samus's contact kills, which one names.</param>
/// <param name="normal">The normal explosion, which two names.</param>
/// <param name="fakeKraid">Fake Kraid's explosion, which three names.</param>
/// <param name="big">The big explosion, which four names.</param>
[CompatibilityService("an enemy death that takes its explosion from a leftover number dies in the explosion the number names in the authored order, or in the small explosion past the last")]
public sealed class LeftoverNumberDeathExplosion<TExplosion>(TExplosion small, TExplosion samusContact, TExplosion normal, TExplosion fakeKraid, TExplosion big)
{
    /// <summary>The death explosions in the order the leftover numbers zero through four name them.</summary>
    private readonly TExplosion[] _numbered = [small, samusContact, normal, fakeKraid, big];

    /// <summary>
    /// The explosion <paramref name="leftoverNumber"/> names: zero through four the small, Samus contact, normal, Fake
    /// Kraid and big explosions, and any later number, or a leftover that is no number, the small one.
    /// </summary>
    /// <param name="leftoverNumber">The number left over for the death, zero or more, or null when what is left is no number.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="leftoverNumber"/> is negative.</exception>
    public TExplosion Named(int? leftoverNumber) =>
        leftoverNumber is int number && Ensure.AtLeastZero(number) < _numbered.Length ? _numbered[number] : _numbered[0];

    /// <summary>
    /// The explosion a death names that takes its number from the enemy projectile pool place a spawn last took (#2474):
    /// the game leaves twice the place, so the first three places name the small, normal and big explosions and any later
    /// place the small one; a spawn into a full pool, which leaves no place, also names the small one.
    /// </summary>
    /// <param name="projectilePlace">The zero-based pool place the spawn last took, or null when the pool was full.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="projectilePlace"/> is negative.</exception>
    public TExplosion LeftByProjectilePlace(int? projectilePlace) => Named(projectilePlace * 2);
}
