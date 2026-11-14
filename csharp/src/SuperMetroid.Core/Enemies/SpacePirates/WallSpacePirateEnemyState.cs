using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of a wall Pirate; the jump endpoints and angular step are the Pirate's own state, which nothing else
/// reads.
/// </summary>
public sealed class WallSpacePirateEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WallSpacePirateSegment> Playback { get; set; }

    /// <summary>
    /// AI entry, initialized for the starting wall and replaced by an animation cue as attack, jump, and landing animations
    /// advance.
    /// </summary>
    public WallSpacePirateAction? Action { get; internal set; }

    /// <summary>
    /// Binary direction; starts downward, reverses on a blocked vertical move, and is reselected from whether the
    /// random sample is odd by the climb animation's direction cues.
    /// </summary>
    public WallSpacePirateClimbDirection ClimbDirection { get; internal set; }

    /// <summary>
    /// Whole room-pixel jump-ellipse center, set when a jump starts: X is the departure X plus or minus half parameter
    /// two and Y the departure Y.
    /// </summary>
    public Position16<RoomPixels> WallJumpArcCenter { get; internal set; }

    /// <summary>
    /// The jump ellipse's angle, which wraps over a whole turn: a quarter turn for a rightward jump or three quarters
    /// for a leftward jump, stepped after each position calculation. A rightward jump steps down through zero to its
    /// endpoint, so the wrap is load-bearing.
    /// </summary>
    public RotationAngle WallJumpArcAngle { get; internal set; }

    /// <summary>The rightward jump's endpoint, step 190 of the turn for slow jumps or 192 for fast ones; reaching it installs the right-wall landing.</summary>
    public RotationAngle RightJumpTargetAngle { get; internal set; }
    /// <summary>The leftward jump's endpoint, step 66 of the turn for slow jumps or 64 for fast ones; reaching it installs the left-wall landing.</summary>
    public RotationAngle LeftJumpTargetAngle { get; internal set; }
    /// <summary>Steps (256ths of a turn) the jump angle turns each update, two for slow or four for fast jumps; subtracted when jumping right and added when jumping left.</summary>
    public int JumpAngleDelta { get; internal set; }
}
