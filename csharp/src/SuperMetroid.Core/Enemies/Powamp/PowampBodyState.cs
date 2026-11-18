using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The state of a Powamp's body (#1845), the half that moves: it rests underground, inflates its balloon and rises with a
/// wiggle, deflates and sinks back, and carries both halves through death.
/// </summary>
public sealed class PowampBodyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal PowampBodyState(PowampAction action) => Action = action;

    /// <summary>The body's pulse animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PowampSegment> Playback { get; set; }

    /// <summary>The body's vertical displacement per AI update; negative values move upward.</summary>
    public SignedSixteenSixteen YVelocity { get; internal set; }

    /// <summary>The body's sideways offset in its wiggle, which it takes next; a new body starts at the centre.</summary>
    public PowampWiggle Wiggle { get; internal set; } = PowampWiggle.First;

    /// <summary>
    /// The updates until the body's next wiggle step, the one it steps on included: one when it is placed, so its first
    /// wiggle update steps, and five after each step.
    /// </summary>
    public int WiggleTimer { get; internal set; } = PowampMotionDefinitions.FirstWiggleStepWait;

    /// <summary>
    /// The updates left of the body's rest, inflation, deflation or death, the one it ends on included: sixty for rest, ten
    /// for inflation or deflation and thirty-two for death, counted down once per update of the action.
    /// </summary>
    public int ActionTimer { get; internal set; }

    /// <summary>The main-AI behaviour.</summary>
    public PowampAction Action { get; internal set; }

    /// <summary>
    /// Whether the body is dying (#2474): a lethal shot has started its own death, which refuses Samus's later touch and
    /// shots whatever its placement says.
    /// </summary>
    internal bool IsDying => Action is PowampAction.FatalDamage or PowampAction.DeathSequence;
}
