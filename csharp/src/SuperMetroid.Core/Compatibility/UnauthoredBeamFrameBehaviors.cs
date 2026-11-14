namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The frame behaviour a Spazer and Plasma beam's shot runs (#1751). The fire routines author a frame behaviour for the
/// uncharged and the charged shot of each of the twelve beam combinations retail equipment can hold; a combination that
/// pairs Spazer with Plasma, which only the pause screen's same-frame toggle can equip, runs what follows them.
/// Uncharged, the Wave pairing's shot (the Chainsaw) runs a Power Bomb's fuse over itself, the Ice pairing's shot
/// (Spacetime) overwrites saved progression from an animation that reaches far enough, and the plain and all-beams
/// pairings' shots run what an unconnected region or the game's live state holds. Charged, the Wave and Ice pairings'
/// shots run what Samus's changing state holds, the all-beams shot (the Murder Beam) starts in the middle of an
/// unrelated part of the game, which its safe left-facing shot, having no animation, never runs, and the plain pairing's
/// shot runs what the game's live state holds.
/// </summary>
/// <remarks>
/// <para>
/// A behaviour that runs data rather than the game's code is outside the emulation scope limit (#1500): reproducing it
/// needs full memory and processor emulation, so the domain names it and refuses the shot where it would run.
/// </para>
/// <para>
/// A rule with no state, held by the beam frame behaviour catalog. Consulted whenever such a beam is fired, after its
/// muzzle collision, as its frame behaviour is installed, and by the import when it decodes the progression the Spacetime
/// shot overwrites. Evidence: the uncharged and charged fire routines index their frame-behaviour lists by the four-bit
/// combination with no bound, so the four Spazer and Plasma combinations read the four words after the twelve each list
/// authors; the beam callback table check (<c>BeamCallbackTablesVerification</c>) compares all thirty-two words with
/// the original lists and what follows them.
/// </para>
/// </remarks>
/// <typeparam name="TBeams">A beam combination.</typeparam>
/// <typeparam name="TBehavior">The frame behaviour a shot runs.</typeparam>
[CompatibilityService("a Spazer and Plasma beam's shot runs the frame behaviour that follows the beam frame-behaviour lists")]
public sealed class UnauthoredBeamFrameBehaviors<TBeams, TBehavior>
    where TBeams : notnull
    where TBehavior : notnull
{
    /// <summary>The frame behaviour each combination's uncharged shot runs.</summary>
    private readonly IReadOnlyDictionary<TBeams, TBehavior> _uncharged;

    /// <summary>The frame behaviour each combination's charged shot runs.</summary>
    private readonly IReadOnlyDictionary<TBeams, TBehavior> _charged;

    /// <summary>Keeps the frame behaviour each combination's shots run.</summary>
    /// <param name="uncharged">The frame behaviour each combination's uncharged shot runs.</param>
    /// <param name="charged">The frame behaviour each combination's charged shot runs.</param>
    /// <exception cref="ArgumentException">The two sets name different combinations, or none.</exception>
    public UnauthoredBeamFrameBehaviors(IReadOnlyDictionary<TBeams, TBehavior> uncharged, IReadOnlyDictionary<TBeams, TBehavior> charged)
    {
        Ensure.NotNull(uncharged);
        Ensure.NotNull(charged);
        if (uncharged.Count == 0 || uncharged.Count != charged.Count || uncharged.Keys.Any(beams => !charged.ContainsKey(beams)))
            throw new ArgumentException("Every combination needs an uncharged and a charged frame behaviour.", nameof(charged));
        _uncharged = uncharged;
        _charged = charged;
    }

    /// <summary>The frame behaviour a shot of <paramref name="beams"/> runs, charged when <paramref name="charged"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The fire routines author the combination's frame behaviours.</exception>
    public TBehavior FrameBehavior(TBeams beams, bool charged) =>
        (charged ? _charged : _uncharged).TryGetValue(beams, out TBehavior? behavior)
            ? behavior
            : throw new ArgumentOutOfRangeException(nameof(beams), beams, "The fire routines author this combination's frame behaviours.");
}
