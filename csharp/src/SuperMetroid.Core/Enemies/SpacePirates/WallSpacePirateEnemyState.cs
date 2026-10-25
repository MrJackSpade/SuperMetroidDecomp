using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of a wall Pirate; the jump endpoints and angular step are the Pirate's own state, which nothing else
/// reads. The values the cartridge keeps in the enemy's common record are emptied with it when the enemy is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class WallSpacePirateEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WallSpacePirateProgram> Program { get; set; }

    /// <summary>
    /// Bank-$B2 AI entry, initialized for the starting wall and replaced by instruction $B2:EF83 as attack, jump, and
    /// landing lists advance; none once a kill has emptied the record.
    /// </summary>
    public WallSpacePirateFunction? Function { get; internal set; }

    /// <summary>
    /// The room position the prepare-jump cues aim the jump at: the departure X plus or minus parameter two. Only the X
    /// axis is stored; no gameplay reads it back.
    /// </summary>
    public Position16<RoomPixels> WallJumpDestination
    {
        get;
        internal set => field = new(value.X, 0);
    }

    /// <summary>
    /// Binary direction; starts downward, reverses on a blocked vertical move, and is reselected from RNG bit zero by
    /// the climb-list direction opcodes.
    /// </summary>
    public WallSpacePirateClimbDirection ClimbDirection { get; internal set; }

    /// <summary>
    /// Whole room-pixel jump-ellipse center, set by $B2:EED4/$EEFD: X is the departure X plus or minus half parameter
    /// two and Y the departure Y.
    /// </summary>
    public Position16<RoomPixels> WallJumpArcCenter { get; internal set; }

    /// <summary>
    /// The jump ellipse's angle, which wraps over a whole turn: a quarter turn for a rightward jump or three quarters
    /// for a leftward jump, stepped after each position calculation. A rightward jump steps down through zero to its
    /// endpoint, so the wrap is load-bearing.
    /// </summary>
    public RotationAngle WallJumpArcAngle { get; internal set; }

    /// <summary>The rightward jump's endpoint, table index 190 for slow jumps or 192 for fast ones; reaching it installs the right-wall landing.</summary>
    public RotationAngle RightJumpTargetAngle { get; internal set; }
    /// <summary>The leftward jump's endpoint, table index 66 for slow jumps or 64 for fast ones; reaching it installs the left-wall landing.</summary>
    public RotationAngle LeftJumpTargetAngle { get; internal set; }
    /// <summary>Table units the jump angle steps each update, two for slow or four for fast jumps; subtracted when jumping right and added when jumping left.</summary>
    public int JumpAngleDelta { get; internal set; }
    /// <summary>Host diagnostic count of successfully allocated lasers since this enemy state was initialized; shared projectile-pool exhaustion does not increment it.</summary>
    public int SpawnedLaserCount { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        WallJumpDestination = Position16<RoomPixels>.Zero;
        ClimbDirection = default;
        WallJumpArcCenter = Position16<RoomPixels>.Zero;
        WallJumpArcAngle = RotationAngle.FromTableIndex(0);
    }
}
