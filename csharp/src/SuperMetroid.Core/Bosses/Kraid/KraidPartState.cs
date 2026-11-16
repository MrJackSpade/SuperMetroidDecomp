using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What every one of Kraid's parts holds: the encounter it is part of, its typed animation and the phase timer its
/// timed phases wait on. Each part kind keeps the phase it runs, of its own type (#2208), and its own state on top of
/// this: his body (<see cref="KraidBodyState"/>), his arm (<see cref="KraidArmState"/>), each lint
/// (<see cref="KraidLintState"/>), his foot (<see cref="KraidFootState"/>) and each fingernail
/// (<see cref="KraidNailState"/>).
/// </summary>
public abstract class KraidPartState
{
    /// <summary>Makes a part of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the part is part of.</param>
    private protected KraidPartState(KraidEnemyState kraid) => Kraid = kraid;

    /// <summary>The encounter the part is part of, which the part holds from its setup (#2218).</summary>
    internal KraidEnemyState Kraid { get; }

    /// <summary>The part's playing animation and where it stands; before the first start, the foot's initial animation.</summary>
    internal AnimationPlayback<KraidPartSegment> Playback { get; set; } =
        new(KraidAnimations.StartingWith(KraidPartSegment.FootInitial), AnimationPlace.StartOfAnimation);

    /// <summary>The updates before the part's timed phase moves on to the phase it names.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>
    /// Counts the phase timer down one update, from a running timer only: whether it ran out on this update, which moves
    /// the timed phase on. A timer at zero stays there, and the phase waits on it.
    /// </summary>
    internal bool CountPhaseTimerDown() => PhaseTimer != 0 && --PhaseTimer == 0;
}
