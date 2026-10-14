using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Debugger-facing view of the native wall Pirate work words. Variables A-F remain backed
/// directly by the enemy slot, while the three bank-$7E:7800 scratch arrays are retained as
/// explicit host fields because they do not have ordinary per-slot variable aliases.
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

    /// <summary>Debug-only destination X written by the two prepare-jump opcodes.</summary>
    public ushort WallJumpDestinationX
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
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

    /// <summary>Slot variable F's byte-angle phase, wrapping over 0..255: starts at $40 for a rightward jump or $C0 for a leftward jump and advances after each whole-position calculation.</summary>
    public ushort WallJumpArcAngle
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Native $7E:8000 per-enemy rightward endpoint: $BE for slow jumps or $C0 for fast jumps; equality after decreasing the angle installs the right-wall landing list.</summary>
    public ushort RightJumpTargetAngle { get; internal set; }
    /// <summary>Native $7E:8002 per-enemy leftward endpoint: $42 for slow jumps or $40 for fast jumps; equality after increasing the angle installs the left-wall landing list.</summary>
    public ushort LeftJumpTargetAngle { get; internal set; }
    /// <summary>Native $7E:8004 per-enemy byte-angle increment magnitude, two for slow or four for fast jumps; subtracted when jumping right and added when jumping left.</summary>
    public ushort JumpAngleDelta { get; internal set; }
    /// <summary>Host diagnostic count of successfully allocated lasers since this enemy state was initialized; shared projectile-pool exhaustion does not increment it.</summary>
    public int SpawnedLaserCount { get; internal set; }
}
