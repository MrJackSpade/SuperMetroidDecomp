using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The state of one Choot. Every value it reads it has set itself: setup sets the spawn and falling positions and the
/// ascent's starting place, activation the jump delay and the apex the falling origin.
/// </summary>
public sealed class ChootEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>, falling along <paramref name="fallingPath"/> after each apex.</summary>
    internal ChootEnemyState(ChootAction action, ChootFallingPath fallingPath)
    {
        Action = action;
        FallingPath = fallingPath;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ChootSegment> Playback { get; set; }

    /// <summary>The behaviour selected by Choot's main AI step.</summary>
    public ChootAction Action { get; internal set; }

    /// <summary>
    /// The ascent's whole-mark place along the quadratic speed curve, moving back two marks per jumping update; the update
    /// that takes it before the curve's start is the apex. It starts at the even mark whose integrated speeds cover the jump
    /// height, at most the curve's last mark.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }

    /// <summary>The falling pattern's current frame, advanced by one per falling update and restarted at each loop's end.</summary>
    public int FallingPatternFrame { get; internal set; }

    /// <summary>
    /// The falling pattern's loops left, the current one included: its population's loop count, at least one, and the
    /// loop end that takes it to zero ends the fall (#2281).
    /// </summary>
    public int FallingPatternLoopCounter { get; internal set; }

    /// <summary>The authored falling path the Choot follows after its apex.</summary>
    internal ChootFallingPath FallingPath { get; private set; }

    /// <summary>Whole-pixel Y origin advance applied between falling-pattern loops, the pattern's authored 28 to 32 pixels.</summary>
    public int FallingPatternYDistance { get; internal set; }

    /// <summary>The initial room position in whole pixels, restored after the last falling-pattern loop.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Spawn X, and spawn Y less the jump height: where the Choot stands at the top of its jump and starts falling from.</summary>
    public Position16<RoomPixels> InitialFallingPosition { get; internal set; }

    /// <summary>The whole-pixel base added to each signed falling-pattern offset; initialized at the apex, and its Y advanced by <see cref="FallingPatternYDistance"/> at every loop's end.</summary>
    public Position16<RoomPixels> FallingOrigin { get; internal set; }

    /// <summary>The ascent's starting place calculated from the population's loop count and pattern height; copied back to <see cref="YSpeedCurvePlace"/> when the jump cycle finishes.</summary>
    public QuadraticSpeedCurvePlace InitialYSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The updates left before the jump, the one it starts on included: the population's jump delay and the update it runs
    /// out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>), so a delay of zero jumps on the first preparation update
    /// (#2281).
    /// </summary>
    public int JumpDelayTimer { get; internal set; }
}
