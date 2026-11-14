namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The sound a Spazer and Plasma beam fires with (#1751). The beam sound lists author an uncharged and a charged firing
/// sound for each of the twelve beam combinations retail equipment can hold; a combination that pairs Spazer with Plasma,
/// which only the pause screen's same-frame toggle can equip, fires with what follows them. Uncharged, it plays the
/// charged Power, Wave, Ice or Ice-Wave beam's sound, in the order the combinations count (plain, Wave, Ice, Ice-Wave);
/// charged, it plays no sound, the missile's, the Super Missile's or no sound in that order. The sound enters Samus's
/// sound queue as any shot's does, so it takes the queue's place and is heard.
/// </summary>
/// <remarks>
/// A rule with no state, held by the projectile sound routing catalog. Consulted whenever such a beam is fired, uncharged
/// or charged, as the shot's sound is chosen. Evidence: the uncharged and charged fire routines index their sound tables
/// by the four-bit combination with no bound, so the four Spazer and Plasma combinations read the four entries after the
/// twelve each table authors, the uncharged table running into the charged one and the charged one into the missile
/// sounds; the projectile sound routing check (<c>ProjectileSoundRoutingVerification</c>) compares every combination's
/// sound, uncharged and charged, with the original tables and what follows them.
/// </remarks>
/// <typeparam name="TBeams">A beam combination.</typeparam>
/// <typeparam name="TSound">A firing sound.</typeparam>
[CompatibilityService("a Spazer and Plasma beam fires with the sounds that follow the beam sound lists")]
public sealed class UnauthoredBeamSounds<TBeams, TSound>
    where TBeams : notnull
    where TSound : struct
{
    /// <summary>The sound each combination's uncharged shot fires with, or none.</summary>
    private readonly IReadOnlyDictionary<TBeams, TSound?> _uncharged;

    /// <summary>The sound each combination's charged shot fires with, or none.</summary>
    private readonly IReadOnlyDictionary<TBeams, TSound?> _charged;

    /// <summary>Keeps the sound each combination fires with.</summary>
    /// <param name="uncharged">The sound each combination's uncharged shot fires with, or none.</param>
    /// <param name="charged">The sound each combination's charged shot fires with, or none.</param>
    /// <exception cref="ArgumentException">The two sets name different combinations, or none.</exception>
    public UnauthoredBeamSounds(IReadOnlyDictionary<TBeams, TSound?> uncharged, IReadOnlyDictionary<TBeams, TSound?> charged)
    {
        Ensure.NotNull(uncharged);
        Ensure.NotNull(charged);
        if (uncharged.Count == 0 || uncharged.Count != charged.Count || uncharged.Keys.Any(beams => !charged.ContainsKey(beams)))
            throw new ArgumentException("Every combination needs an uncharged and a charged sound.", nameof(charged));
        _uncharged = uncharged;
        _charged = charged;
    }

    /// <summary>The sound a shot of <paramref name="beams"/> fires with, charged when <paramref name="charged"/>, or none.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The beam sound lists author the combination's sounds.</exception>
    public TSound? Sound(TBeams beams, bool charged) =>
        (charged ? _charged : _uncharged).TryGetValue(beams, out TSound? sound)
            ? sound
            : throw new ArgumentOutOfRangeException(nameof(beams), beams, "The beam sound lists author this combination's sounds.");
}
