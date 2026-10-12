using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Proven parameter-one bits consumed by wall Space Pirate code. The low bit chooses the
/// starting wall; the high bit independently selects the slower retail jump/laser timing.
/// No other parameter-one bits are interpreted by <c>$B2:EF9F-$F11F</c>.
/// </summary>
[Flags]
public enum WallSpacePirateParameterFlags : ushort
{
    /// <summary>Parameter-one bit zero tested by $B2:EF9F: starts the downward-climbing list and detection function on the right wall; clearing it selects the left wall.</summary>
    StartsOnRightWall = 0x0001,
    /// <summary>Parameter-one bit fifteen preserves the native slow jump settings: two byte-angle units per update with right/left endpoint angles $BE/$42 instead of the fast $C0/$40 endpoints.</summary>
    SlowJumpAndLaser = 0x8000,
}

/// <summary>Literal bank-$B2 function pointers stored in wall Pirate variable A.</summary>
public enum WallSpacePirateFunction : ushort
{
    /// <summary>Watch Samus while continuing the left-wall climbing animation.</summary>
    ClimbingLeftWall = 0xf034,

    /// <summary>The attack list owns presentation until it prepares a rightward jump.</summary>
    AttackAnimationOnLeftWall = 0xf04f,

    /// <summary>Follow the cartridge's sine/cosine arc from the left wall to the right.</summary>
    WallJumpingRight = 0xf050,

    /// <summary>Watch Samus while continuing the right-wall climbing animation.</summary>
    ClimbingRightWall = 0xf0c8,

    /// <summary>The attack list owns presentation until it prepares a leftward jump.</summary>
    AttackAnimationOnRightWall = 0xf0e3,

    /// <summary>Follow the cartridge's sine/cosine arc from the right wall to the left.</summary>
    WallJumpingLeft = 0xf0e4,
}

/// <summary>The binary climb direction stored in native variable C.</summary>
public enum WallSpacePirateClimbDirection : ushort
{
    /// <summary>Variable-C value zero selects the downward climb list, whose movement opcodes add three room pixels to Y before collision handling.</summary>
    Down = 0,
    /// <summary>Variable-C value one selects the upward climb list, whose movement opcodes subtract three room pixels from Y before collision handling.</summary>
    Up = 1,
}

/// <summary>
/// Debugger-facing view of the native wall Pirate work words. Variables A-F remain backed
/// directly by the enemy slot, while the three bank-$7E:7800 scratch arrays are retained as
/// explicit host fields because they do not have ordinary per-slot variable aliases.
/// </summary>
public sealed class WallSpacePirateEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new wall space pirate enemy state instance.</summary>
    internal WallSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

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
