using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The state of one half of a Powamp: a nonzero population parameter one makes the balloon, while zero is the
/// immediately following moving body. The cartridge reuses the same record words for different values in each half;
/// here each half's values have their own members. Every value here is one the cartridge keeps in the enemy's
/// common record, so it is emptied with that record when the half is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class PowampEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<PowampProgram> Program { get; set; }

    /// <summary>Whether this half is the balloon (population parameter one nonzero) rather than the moving body.</summary>
    public bool IsBalloon { get; internal set; }

    /// <summary>The body's vertical displacement per AI update; negative values move upward.</summary>
    public SnesSignedSixteenSixteen YVelocity { get; internal set; }

    /// <summary>The balloon's original room position: X is both halves' horizontal wiggle center and Y feeds the body's resting and rise-target calculations.</summary>
    public Position16<RoomPixels> BalloonSpawn { get; internal set; }

    /// <summary>The body's zero-based phase 0 through 11 in the signed three-pixel wiggle wave; phases zero and six are centered.</summary>
    public int WiggleIndex { get; internal set; }

    /// <summary>The body's AI-update countdown, restarted at five for each horizontal wiggle offset.</summary>
    public NativeCountdown WiggleTimer { get; internal set; }

    /// <summary>
    /// The balloon's grappled rise in room pixels, copied from population parameter two (0 through 65535) and
    /// subtracted from the balloon's spawn Y, wrapping at the room's top edge, to obtain the grappled body's target.
    /// </summary>
    public int BalloonGrappleTravelDistance { get; internal set; }

    /// <summary>The body's countdown, decremented once per relevant main-AI invocation; initialized to 60 for rest, ten for inflation/deflation, or 32 for death, and expires at zero or signed underflow.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }

    /// <summary>The main-AI behaviour; balloons keep the no-op, and none once a kill has emptied the record.</summary>
    public PowampEnemyFunction? Function { get; internal set; }

    /// <summary>Empties every value, when a kill empties the half's common record.</summary>
    internal void ClearWithRecord()
    {
        IsBalloon = false;
        YVelocity = SnesSignedSixteenSixteen.Zero;
        BalloonSpawn = Position16<RoomPixels>.Zero;
        WiggleIndex = 0;
        WiggleTimer = NativeCountdown.Start(0);
        BalloonGrappleTravelDistance = 0;
        FunctionTimer = NativeCountdown.Start(0);
        Function = null;
    }
}
