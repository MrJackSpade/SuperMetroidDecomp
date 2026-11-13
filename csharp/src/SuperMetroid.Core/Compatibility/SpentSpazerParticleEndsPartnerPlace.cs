namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a Spazer special-attack particle that has struck something ends besides itself while it still circles or flies
/// (#1683): the shot holding the place its partner took, which is the partner itself until a shinespark crash echo is
/// released onto that place, when the echo ends instead. Replacing the partner has already ended it, so the echo is
/// the only shot the rule can end there, and the spent particle's hit takes Samus's crash echo with it.
/// </summary>
/// <remarks>
/// Consulted once per spent particle, on its update, after the particle itself ends. Evidence: the original's spent
/// branch of the Spazer particle's update deletes its own projectile and then the projectile two places from it,
/// whatever projectile that place holds; a crash echo released while the attack runs takes the last two places, the
/// first of which the attack's last trailing particle holds.
/// </remarks>
[CompatibilityService("a spent Spazer particle ends whatever shot holds its partner's place")]
public static class SpentSpazerParticleEndsPartnerPlace
{
    /// <summary>The shot to end with the spent particle.</summary>
    /// <typeparam name="TShot">The shot type.</typeparam>
    /// <param name="partner">The spent particle's partner, as the attack paired them.</param>
    /// <param name="holderOfPlaceOf">The shot now holding the place a given shot took.</param>
    /// <returns>The shot holding the partner's place.</returns>
    public static TShot ShotToEnd<TShot>(TShot partner, Func<TShot, TShot> holderOfPlaceOf) where TShot : class =>
        holderOfPlaceOf(partner);
}
