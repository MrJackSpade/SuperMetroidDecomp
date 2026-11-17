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

    /// <summary>The body's AI-update countdown, restarted at five for each horizontal wiggle offset.</summary>
    public int WiggleTimer { get; internal set; }

    /// <summary>The updates left of the body's current wait: 60 at rest, ten while inflating or deflating, 32 while dying; it ends on the update it reaches zero.</summary>
    public int ActionTimer { get; internal set; }

    /// <summary>The main-AI behaviour.</summary>
    public PowampAction Action { get; internal set; }

    /// <summary>Whether touch and shot AI are refused: set once a lethal shot starts the body's death.</summary>
    public bool RefusesContact { get; internal set; }
}
