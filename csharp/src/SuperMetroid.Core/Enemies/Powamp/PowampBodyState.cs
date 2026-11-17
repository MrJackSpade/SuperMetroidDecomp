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

    /// <summary>The body's phase 0 through 11 in the signed three-pixel wiggle wave; the wiggle is centred where its offset is zero.</summary>
    public int WiggleIndex { get; internal set; }

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

    /// <summary>Whether touch and shot AI are refused: set once a lethal shot starts the body's death.</summary>
    public bool RefusesContact { get; internal set; }
}
