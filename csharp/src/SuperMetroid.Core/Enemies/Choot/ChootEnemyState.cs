using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The state of one Choot. Every value is the Choot's own: setup writes the spawn and falling positions and the
/// starting ascent index, activation writes the jump delay and the apex writes the falling origin, each before it is
/// read, so nothing a previous Choot in the slot left reaches a new one.
/// </summary>
public sealed class ChootEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ChootSegment> Playback { get; set; }

    /// <summary>The behaviour selected by Choot's main AI step.</summary>
    public ChootAction? Action { get; internal set; }

    /// <summary>
    /// The ascent's whole-row place along the quadratic speed table, moving back two rows per jumping update; the update
    /// that takes it before the first row is the apex. It starts at the even row whose integrated speeds cover the jump
    /// height, at most row 94.
    /// </summary>
    public EnemyQuadraticSpeedIndex YSpeedCurvePlace { get; internal set; }

    /// <summary>The falling pattern's current frame, advanced by one per falling update and restarted at each loop's end.</summary>
    public int FallingPatternFrame { get; internal set; }

    /// <summary>Loop counter initialized to its authored pattern loops minus one, expiring when it decrements below zero.</summary>
    public int FallingPatternLoopCounter { get; internal set; }

    /// <summary>The authored falling path the Choot follows after its apex.</summary>
    internal ChootFallingPath? FallingPath { get; set; }

    /// <summary>Whole-pixel Y origin advance applied between falling-pattern loops, the pattern's authored 28 to 32 pixels.</summary>
    public int FallingPatternYDistance { get; internal set; }

    /// <summary>The initial room position in whole pixels, restored after the last falling-pattern loop.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Spawn X and the wrapped whole-pixel spawn Y minus the computed jump height, installed as the position and falling origin at the apex transition.</summary>
    public Position16<RoomPixels> InitialFallingPosition { get; internal set; }

    /// <summary>The whole-pixel base added to each signed falling-pattern offset; initialized at the apex, and its Y advanced by <see cref="FallingPatternYDistance"/> at every loop's end.</summary>
    public Position16<RoomPixels> FallingOrigin { get; internal set; }

    /// <summary>The ascent's starting place calculated from the population's loop count and pattern height; copied back to <see cref="YSpeedCurvePlace"/> when the jump cycle finishes.</summary>
    public EnemyQuadraticSpeedIndex InitialYSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The jump delay's countdown. The jump begins only after decrementing through zero to
    /// below zero, so a population delay of zero launches on the first preparation frame.
    /// </summary>
    public int JumpDelayTimer { get; internal set; }
}
