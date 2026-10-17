using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Debugger-facing view of the wall Pirate's state. Variables A-F remain backed directly by the enemy slot, whose words
/// other enemies can read as enemy RAM, so each typed view keeps its word exact; the jump endpoints and angular step
/// are the Pirate's own state, which nothing else reads.
/// </summary>
public sealed class WallSpacePirateEnemyState
{
    /// <summary>Physical enemy slot backing native variables A-F for this wall Pirate.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects one wall Pirate slot into typed debugger state.</summary>
    internal WallSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WallSpacePirateProgram> Program { get; set; }

    /// <summary>Bank-$B2 AI entry stored in slot variable A, initialized for the starting wall and replaced by instruction $B2:EF83 as attack, jump, and landing lists advance.</summary>
    public WallSpacePirateFunction Function
    {
        get => (WallSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// The room position the prepare-jump cues aim the jump at (slot variable B): the departure X plus or minus
    /// parameter two. Only the X axis is stored; no gameplay reads it back.
    /// </summary>
    public Position16<RoomPixels> WallJumpDestination
    {
        get => new(_slot.VariableB, 0);
        internal set => _slot.VariableB = value.X;
    }

    /// <summary>Binary direction in slot variable C; starts downward, reverses on a blocked vertical move, and is reselected from RNG bit zero by the climb-list direction opcodes.</summary>
    public WallSpacePirateClimbDirection ClimbDirection
    {
        get => (WallSpacePirateClimbDirection)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>
    /// Whole room-pixel jump-ellipse center in slot variables D/E, set by $B2:EED4/$EEFD: X is the
    /// departure X plus or minus half parameter two and Y the departure Y.
    /// </summary>
    public Position16<RoomPixels> WallJumpArcCenter
    {
        get => new(_slot.VariableD, _slot.VariableE);
        internal set
        {
            _slot.VariableD = value.X;
            _slot.VariableE = value.Y;
        }
    }

    /// <summary>
    /// The jump ellipse's angle in slot variable F, which wraps over a whole turn: a quarter turn for a rightward jump or
    /// three quarters for a leftward jump, stepped after each position calculation. A rightward jump steps down through
    /// zero to its endpoint, so the wrap is load-bearing. The slot word holds the angle's table index.
    /// </summary>
    public SnesAngle WallJumpArcAngle
    {
        get => SnesAngle.FromTableIndex(checked((byte)_slot.VariableF));
        internal set => _slot.VariableF = value.TableIndex;
    }

    /// <summary>The rightward jump's endpoint, table index 190 for slow jumps or 192 for fast ones; reaching it installs the right-wall landing.</summary>
    public SnesAngle RightJumpTargetAngle { get; internal set; }
    /// <summary>The leftward jump's endpoint, table index 66 for slow jumps or 64 for fast ones; reaching it installs the left-wall landing.</summary>
    public SnesAngle LeftJumpTargetAngle { get; internal set; }
    /// <summary>Table units the jump angle steps each update, two for slow or four for fast jumps; subtracted when jumping right and added when jumping left.</summary>
    public int JumpAngleDelta { get; internal set; }
    /// <summary>Host diagnostic count of successfully allocated lasers since this enemy state was initialized; shared projectile-pool exhaustion does not increment it.</summary>
    public int SpawnedLaserCount { get; internal set; }
}
