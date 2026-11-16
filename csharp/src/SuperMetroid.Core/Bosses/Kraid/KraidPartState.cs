using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What every one of Kraid's parts runs: its typed animation, the phase it runs and the phase timer that
/// resumes its next phase. Each part kind keeps its own state on top of this in its own type: his body (<see
/// cref="KraidBodyState"/>), his arm (<see cref="KraidArmState"/>), each lint (<see cref="KraidLintState"/>), his foot
/// (<see cref="KraidFootState"/>) and each fingernail (<see cref="KraidNailState"/>).
/// </summary>
public abstract class KraidPartState
{
    /// <summary>The part's playing animation and where it stands; before the first start, the foot's initial animation.</summary>
    internal AnimationPlayback<KraidPartSegment> Playback { get; set; } =
        new(KraidAnimations.StartingWith(KraidPartSegment.FootInitial), AnimationPlace.StartOfAnimation);

    /// <summary>The phase a part resumes when its phase timer expires; a part that resumes none rests.</summary>
    public KraidPhase PhaseToResume { get; internal set; } = KraidPhase.Resting;

    /// <summary>
    /// The phase the part runs. A part that has stopped (the arm while rising, the foot between its walks and once Kraid
    /// grows or dies) rests, doing nothing on its turn (#1870); every part rests until it is given a phase.
    /// </summary>
    public KraidPhase Phase { get; internal set; } = KraidPhase.Resting;

    /// <summary>The updates before the part's phase timer installs <see cref="PhaseToResume"/>.</summary>
    public int PhaseTimer { get; internal set; }
}
