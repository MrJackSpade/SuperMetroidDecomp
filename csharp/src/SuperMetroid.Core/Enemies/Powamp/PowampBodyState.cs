using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The state of a Powamp's body (#1845), the half that moves: it rests underground, inflates its balloon and rises with a
/// wiggle, deflates and sinks back, and carries both halves through death.
/// </summary>
public sealed class PowampBodyState
{
    /// <summary>The body's pulse animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PowampSegment> Playback { get; set; }

    /// <summary>The body's vertical displacement per AI update; negative values move upward.</summary>
    public SignedSixteenSixteen YVelocity { get; internal set; }

    /// <summary>The body's phase 0 through 11 in the signed three-pixel wiggle wave; the wiggle is centred where its offset is zero.</summary>
    public int WiggleIndex { get; internal set; }

    /// <summary>The body's AI-update countdown, restarted at five for each horizontal wiggle offset.</summary>
    public int WiggleTimer { get; internal set; }

    /// <summary>The body's countdown, decremented once per relevant main-AI invocation; initialized to 60 for rest, ten for inflation/deflation, or 32 for death, and expires at zero or past it.</summary>
    public int ActionTimer { get; internal set; }

    /// <summary>The main-AI behaviour.</summary>
    public PowampAction? Action { get; internal set; }

    /// <summary>Whether touch and shot AI are refused: set once a lethal shot starts the body's death.</summary>
    public bool RefusesContact { get; internal set; }
}
