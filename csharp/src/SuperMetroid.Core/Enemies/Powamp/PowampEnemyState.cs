using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The state of one half of a Powamp: a nonzero population parameter one makes the balloon, while zero is the
/// immediately following moving body. Each half's values have their own members.
/// </summary>
public sealed class PowampEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PowampProgram> Playback { get; set; }

    /// <summary>Whether this half is the balloon (population parameter one nonzero) rather than the moving body.</summary>
    public bool IsBalloon { get; internal set; }

    /// <summary>The body's vertical displacement per AI update; negative values move upward.</summary>
    public SignedSixteenSixteen YVelocity { get; internal set; }

    /// <summary>The balloon's original room position: X is both halves' horizontal wiggle center and Y feeds the body's resting and rise-target calculations.</summary>
    public Position16<RoomPixels> BalloonSpawn { get; internal set; }

    /// <summary>The body's zero-based phase 0 through 11 in the signed three-pixel wiggle wave; phases zero and six are centered.</summary>
    public int WiggleIndex { get; internal set; }

    /// <summary>The body's AI-update countdown, restarted at five for each horizontal wiggle offset.</summary>
    public int WiggleTimer { get; internal set; }

    /// <summary>
    /// The balloon's grappled rise in room pixels, copied from its spawn settings and
    /// subtracted from the balloon's spawn Y, wrapping at the room's top edge, to obtain the grappled body's target.
    /// </summary>
    public int BalloonGrappleTravelDistance { get; internal set; }

    /// <summary>The body's countdown, decremented once per relevant main-AI invocation; initialized to 60 for rest, ten for inflation/deflation, or 32 for death, and expires at zero or signed underflow.</summary>
    public int FunctionTimer { get; internal set; }

    /// <summary>The main-AI behaviour; balloons keep the no-op.</summary>
    public PowampEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Whether touch and shot AI are refused (parameter two is nonzero): set for a body once a lethal shot starts its
    /// death, and from spawn for a balloon, whose parameter two is its grapple travel distance.
    /// </summary>
    public bool RefusesContact { get; internal set; }
}
