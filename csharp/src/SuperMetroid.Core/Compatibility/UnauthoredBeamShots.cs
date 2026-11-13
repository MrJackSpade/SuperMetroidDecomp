namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a Spazer and Plasma beam fires (#1517). The beam projectile selectors author the twelve beam combinations retail
/// equipment can hold, the uncharged shots and then the charged ones, with the missile and bomb selectors after them; a
/// combination that pairs Spazer with Plasma, which only the pause screen's same-frame toggle can equip, selects past
/// its block. Its uncharged shot is a charged Power, Wave, Ice or Ice-Wave shot, and its charged shot is a missile, a
/// Super Missile or a Power Bomb, carrying the beam's own identity and frame behaviours with that projectile's damage and
/// animation.
/// </summary>
/// <remarks>
/// A rule with no state, held by the projectile selection catalog. Consulted whenever such a beam's shot takes its damage
/// and animation: when it is fired, uncharged or charged, when an enemy reflects it, and when the installation lists the
/// animations an equipped combination's shots can run. Evidence: the fire routines index the selector table by the
/// four-bit combination, so combinations twelve through fifteen read the four entries after the twelve they author; the
/// selection check compares each such entry with the original selectors.
/// </remarks>
/// <typeparam name="TBeams">A beam combination.</typeparam>
/// <typeparam name="TProjectile">The projectile a shot fires: its damage and animation.</typeparam>
[CompatibilityService("a Spazer and Plasma beam fires the projectile selected past the beam selectors")]
public sealed class UnauthoredBeamShots<TBeams, TProjectile>
    where TBeams : notnull
    where TProjectile : struct
{
    /// <summary>The projectile each combination's uncharged shot fires.</summary>
    private readonly IReadOnlyDictionary<TBeams, TProjectile> _uncharged;

    /// <summary>The projectile each combination's charged shot fires.</summary>
    private readonly IReadOnlyDictionary<TBeams, TProjectile> _charged;

    /// <summary>Keeps what each combination fires.</summary>
    /// <param name="uncharged">The projectile each combination's uncharged shot fires.</param>
    /// <param name="charged">The projectile each combination's charged shot fires.</param>
    /// <exception cref="ArgumentException">The two sets name different combinations.</exception>
    public UnauthoredBeamShots(IReadOnlyDictionary<TBeams, TProjectile> uncharged, IReadOnlyDictionary<TBeams, TProjectile> charged)
    {
        Ensure.NotNull(uncharged);
        Ensure.NotNull(charged);
        if (uncharged.Count != charged.Count || uncharged.Keys.Any(beams => !charged.ContainsKey(beams)))
            throw new ArgumentException("Every combination needs an uncharged and a charged shot.", nameof(charged));
        _uncharged = uncharged;
        _charged = charged;
    }

    /// <summary>The projectile a shot of <paramref name="beams"/> fires, charged when <paramref name="charged"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The beam selectors author the combination's shots.</exception>
    public TProjectile Shot(TBeams beams, bool charged) =>
        (charged ? _charged : _uncharged).TryGetValue(beams, out TProjectile projectile)
            ? projectile
            : throw new ArgumentOutOfRangeException(nameof(beams), beams, "The beam selectors author this combination's shots.");
}
