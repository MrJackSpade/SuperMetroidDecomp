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
    /// <summary>Enemy slot whose native variables back the debugger-facing state view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a view over the slot initialized by the wall-Pirate AI.</summary>
    /// <param name="slot">Enemy slot containing the native per-enemy variables.</param>
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

    /// <summary>Whole room-pixel X center in slot variable D, set by $B2:EED4/$EEFD to the departure X plus or minus half parameter two for the jump ellipse.</summary>
    public ushort WallJumpArcCenterX
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Whole room-pixel Y center in slot variable E, captured from departure Y when preparing either jump and used by the cosine-based arc position writer.</summary>
    public ushort WallJumpArcCenterY
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
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

/// <summary>
/// Translation of wall Space Pirates $F353/$F393/$F3D3/$F413/$F453/$F493. This covers the
/// complete retail family: wall climbing, solid collision, random reversals, firing, both
/// wall-jump arcs, animation bytecode, and the shared Pirate/Mother-Brain laser.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Bank-$B2 definition pointer for the grey wall Space Pirate, whose initializer begins at $F353.</summary>
    internal const ushort GreyWallSpacePirateDefinition = 0xf353;
    /// <summary>Bank-$B2 definition pointer for the green wall Space Pirate initializer at $F393.</summary>
    internal const ushort GreenWallSpacePirateDefinition = 0xf393;
    /// <summary>Bank-$B2 definition pointer for the red wall Space Pirate initializer at $F3D3.</summary>
    internal const ushort RedWallSpacePirateDefinition = 0xf3d3;
    /// <summary>Bank-$B2 definition pointer for the gold wall Space Pirate initializer at $F413.</summary>
    internal const ushort GoldWallSpacePirateDefinition = 0xf413;
    /// <summary>Bank-$B2 definition pointer for the magenta wall Space Pirate initializer at $F453.</summary>
    internal const ushort MagentaWallSpacePirateDefinition = 0xf453;
    /// <summary>Bank-$B2 definition pointer for the silver wall Space Pirate initializer at $F493.</summary>
    internal const ushort SilverWallSpacePirateDefinition = 0xf493;

    /// <summary>Strict vertical distance in room pixels within which a wall Pirate can detect Samus and attack.</summary>
    private const int WallPirateSamusDetectionBand = 32;
    /// <summary>Vertical pixel displacement from a Pirate's position to the laser muzzle.</summary>
    private const int WallPirateLaserMuzzleYOffset = 16;
    /// <summary>Library-two sound request queued when a wall Pirate successfully allocates its laser.</summary>
    private const ushort WallPirateLaserSound = 0x0067;
    /// <summary>Library-two sound request emitted by the wall-jump attack instruction.</summary>
    private const ushort WallPirateJumpSound = 0x0066;

    /// <summary>Per-slot state views created when each wall Space Pirate runs its native initializer.</summary>
    private readonly WallSpacePirateEnemyState?[] _wallSpacePirateStates =
        new WallSpacePirateEnemyState?[MaximumEnemyCount];

    /// <summary>Tests whether a bank-$B2 enemy definition pointer identifies one of the six wall-Pirate variants.</summary>
    /// <param name="definition">Enemy definition pointer stored in the room enemy slot.</param>
    /// <returns><see langword="true"/> for a grey, green, red, gold, magenta, or silver wall Pirate.</returns>
    internal static bool IsWallSpacePirateDefinition(ushort definition) => definition is
        GreyWallSpacePirateDefinition or
        GreenWallSpacePirateDefinition or
        RedWallSpacePirateDefinition or
        GoldWallSpacePirateDefinition or
        MagentaWallSpacePirateDefinition or
        SilverWallSpacePirateDefinition;

    /// <summary>Tests whether a definition belongs to any translated ordinary Space Pirate family.</summary>
    /// <param name="definition">Enemy definition pointer to classify.</param>
    /// <returns><see langword="true"/> for a wall, walking, or ninja Space Pirate.</returns>
    internal static bool IsOrdinarySpacePirateDefinition(ushort definition) =>
        IsWallSpacePirateDefinition(definition) ||
        IsWalkingSpacePirateDefinition(definition) ||
        IsNinjaSpacePirateDefinition(definition);

    /// <summary>Ports <c>InitAI_PirateWall</c> at <c>$B2:EF9F</c>.</summary>
    private void InitializeWallSpacePirate(RoomEnemySlot slot)
    {
        var state = new WallSpacePirateEnemyState(slot);
        _wallSpacePirateStates[slot.SlotIndex] = state;

        var flags = (WallSpacePirateParameterFlags)slot.Parameter1;
        bool startsOnRight = (flags & WallSpacePirateParameterFlags.StartsOnRightWall) != 0;
        bool slowJump = (flags & WallSpacePirateParameterFlags.SlowJumpAndLaser) != 0;

        slot.CurrentInstruction = startsOnRight
            ? WallSpacePirateInstructionProgramDefinitions.MovingDownRightWall
            : WallSpacePirateInstructionProgramDefinitions.MovingDownLeftWall;
        state.Function = startsOnRight
            ? WallSpacePirateFunction.ClimbingRightWall
            : WallSpacePirateFunction.ClimbingLeftWall;
        state.ClimbDirection = WallSpacePirateClimbDirection.Down;

        // The USA/Japan build uses byte angles. Bit fifteen selects 63 two-step frames;
        // clearing it selects 32 four-step frames with adjusted endpoints. These values are
        // not approximations: they are the literal $BE/$42/$02 and $C0/$40/$04 branches.
        state.RightJumpTargetAngle = slowJump ? (ushort)190 : (ushort)192;
        state.LeftJumpTargetAngle = slowJump ? (ushort)66 : (ushort)64;
        state.JumpAngleDelta = slowJump ? (ushort)2 : (ushort)4;

        SnapWallSpacePirateXToTile(slot);
    }

    /// <summary>Ports <c>MainAI_PirateWall</c> and its six function targets.</summary>
    private static void RunWallSpacePirateMain(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case WallSpacePirateFunction.ClimbingLeftWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                {
                    InstallWallSpacePirateInstruction(
                        slot,
                        WallSpacePirateInstructionProgramDefinitions.FireAndJumpRight);
                }
                return;

            case WallSpacePirateFunction.ClimbingRightWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                {
                    InstallWallSpacePirateInstruction(
                        slot,
                        WallSpacePirateInstructionProgramDefinitions.FireAndJumpLeft);
                }
                return;

            case WallSpacePirateFunction.AttackAnimationOnLeftWall:
            case WallSpacePirateFunction.AttackAnimationOnRightWall:
                // These are literal RTS functions. Actor presentation and timing are owned
                // entirely by the currently-running instruction list.
                return;

            case WallSpacePirateFunction.WallJumpingRight:
                StepWallSpacePirateJumpRight(slot, state);
                return;

            case WallSpacePirateFunction.WallJumpingLeft:
                StepWallSpacePirateJumpLeft(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Wall Space Pirate function $B2:{(ushort)state.Function:X4} is not translated.");
        }

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Wall Space Pirate detection requires the active Samus position.");
    }

    /// <summary>Advances a rightward jump arc and installs the right-wall landing list at its endpoint angle.</summary>
    /// <param name="slot">Enemy whose position and instruction state are advanced.</param>
    /// <param name="state">Per-enemy arc center and byte-angle timing parameters.</param>
    private static void StepWallSpacePirateJumpRight(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        PositionWallSpacePirateOnArc(slot, state);
        state.WallJumpArcAngle = unchecked((byte)(
            state.WallJumpArcAngle - state.JumpAngleDelta));
        if (state.WallJumpArcAngle != state.RightJumpTargetAngle)
            return;

        InstallWallSpacePirateInstruction(
            slot,
            WallSpacePirateInstructionProgramDefinitions.LandedOnRightWall);
        SnapWallSpacePirateXToTile(slot);
    }

    /// <summary>Advances a leftward jump arc and installs the left-wall landing list at its endpoint angle.</summary>
    /// <param name="slot">Enemy whose position and instruction state are advanced.</param>
    /// <param name="state">Per-enemy arc center and byte-angle timing parameters.</param>
    private static void StepWallSpacePirateJumpLeft(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        PositionWallSpacePirateOnArc(slot, state);
        state.WallJumpArcAngle = unchecked((byte)(
            state.WallJumpArcAngle + state.JumpAngleDelta));
        if (state.WallJumpArcAngle != state.LeftJumpTargetAngle)
            return;

        InstallWallSpacePirateInstruction(
            slot,
            WallSpacePirateInstructionProgramDefinitions.LandedOnLeftWall);
        SnapWallSpacePirateXToTile(slot);
    }

    /// <summary>Recomputes whole-pixel X and Y from the stored center and current native byte-angle phase.</summary>
    /// <param name="slot">Enemy whose whole positions are replaced while preserving fractional positions.</param>
    /// <param name="state">Arc center and angle values established by jump preparation.</param>
    private static void PositionWallSpacePirateOnArc(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        // Native math rewrites both whole positions every frame ($B2:F0F9/$F113) and leaves
        // the fractions untouched. The jump ellipse is parameter2 pixels wide and
        // parameter2/4 pixels tall.
        slot.XPosition = unchecked((ushort)(
            state.WallJumpArcCenterX +
            ReadEightBitNegativeSineProduct(
                state.WallJumpArcAngle,
                (ushort)(slot.Parameter2 >> 1))));
        slot.YPosition = unchecked((ushort)(
            state.WallJumpArcCenterY -
            ReadEightBitCosineProduct(state.WallJumpArcAngle, (ushort)(slot.Parameter2 >> 2))));
    }

    /// <summary>Checks whether Samus is within the wall Pirate's vertical attack band.</summary>
    /// <param name="slot">Wall Pirate whose Y position is the detection reference.</param>
    /// <param name="samus">Active player state to compare with the Pirate.</param>
    /// <returns><see langword="true"/> when the signed vertical separation is less than 32 pixels.</returns>
    private static bool WallSpacePirateCanAttack(RoomEnemySlot slot, SamusState samus) =>
        Math.Abs(unchecked((short)(samus.YPosition - slot.YPosition))) <
            WallPirateSamusDetectionBand;

    /// <summary>Applies the native nibble-dependent eight- or sixteen-pixel X alignment after a jump landing.</summary>
    /// <param name="slot">Enemy whose whole-pixel X coordinate is snapped to the wall grid.</param>
    private static void SnapWallSpacePirateXToTile(RoomEnemySlot slot)
    {
        // `$B2:EFFF` deliberately has two alignment grids. Nibbles 0..10 round down to an
        // eight-pixel boundary; 11..15 round up to the next sixteen-pixel boundary.
        slot.XPosition = (slot.XPosition & 0x000f) < 11
            ? unchecked((ushort)(slot.XPosition & 0xfff8))
            : unchecked((ushort)((slot.XPosition & 0xfff0) + 16));
    }

    /// <summary>Switches the enemy to a wall-Pirate instruction list and starts its first instruction timer.</summary>
    /// <param name="slot">Enemy whose current list and timer are replaced.</param>
    /// <param name="instructionPointer">Bank-$B2 instruction-list address to run next.</param>
    private static void InstallWallSpacePirateInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    /// <summary>Moves the Pirate vertically, advancing past the opcode on success or reversing climb list after collision.</summary>
    /// <param name="slot">Enemy being moved through room collision data.</param>
    /// <param name="state">Per-enemy climb direction updated when movement collides.</param>
    /// <param name="level">Current room collision map; required to resolve the movement.</param>
    /// <param name="signedPixelDisplacement">Native signed pixel displacement encoded in the instruction operand.</param>
    /// <param name="onRightWall">Whether the alternate climb list belongs to the right wall.</param>
    /// <param name="cursor">Instruction cursor updated to the next opcode or selected replacement list.</param>
    private void MoveWallSpacePirateAndReverseOnCollision(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        RoomLevelData? level,
        ushort signedPixelDisplacement,
        bool onRightWall,
        ref ushort cursor)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Wall Space Pirate climbing requires room collision data.");
        }

        int displacement = unchecked((short)signedPixelDisplacement) << 16;
        if (!MoveEnemyVertically(level, slot, displacement))
        {
            cursor = unchecked((ushort)(cursor + 4));
            return;
        }

        // A collision retains the collision helper's clamped position, flips only bit zero,
        // and returns a fresh list pointer. It does not resume after the opcode operand.
        state.ClimbDirection = state.ClimbDirection == WallSpacePirateClimbDirection.Down
            ? WallSpacePirateClimbDirection.Up
            : WallSpacePirateClimbDirection.Down;
        cursor = SelectWallSpacePirateClimbList(onRightWall, state.ClimbDirection);
    }

    /// <summary>Chooses up or down from the next RNG low bit and selects that wall's corresponding climb list.</summary>
    /// <param name="state">Per-enemy state receiving the randomly chosen direction.</param>
    /// <param name="onRightWall">Whether to choose from the right-wall instruction lists.</param>
    /// <param name="cursor">Instruction cursor replaced with the selected list address.</param>
    private void RandomizeWallSpacePirateClimbDirection(
        WallSpacePirateEnemyState state,
        bool onRightWall,
        ref ushort cursor)
    {
        state.ClimbDirection = (_nextRandom!() & 1) == 0
            ? WallSpacePirateClimbDirection.Down
            : WallSpacePirateClimbDirection.Up;
        cursor = SelectWallSpacePirateClimbList(onRightWall, state.ClimbDirection);
    }

    /// <summary>Selects the native climb instruction list for a wall and vertical direction pair.</summary>
    /// <param name="onRightWall">Whether the Pirate is attached to the right wall.</param>
    /// <param name="direction">Upward or downward climb direction stored in variable C.</param>
    /// <returns>Bank-$B2 address of the matching climb list.</returns>
    private static ushort SelectWallSpacePirateClimbList(
        bool onRightWall,
        WallSpacePirateClimbDirection direction) => (onRightWall, direction) switch
    {
        (false, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateInstructionProgramDefinitions.MovingDownLeftWall,
        (false, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateInstructionProgramDefinitions.MovingUpLeftWall,
        (true, WallSpacePirateClimbDirection.Down) =>
            WallSpacePirateInstructionProgramDefinitions.MovingDownRightWall,
        (true, WallSpacePirateClimbDirection.Up) =>
            WallSpacePirateInstructionProgramDefinitions.MovingUpRightWall,
        _ => throw new InvalidOperationException(
            $"Wall Pirate climb direction ${(ushort)direction} is not binary."),
    };

    /// <summary>Stores the destination, ellipse center, and initial byte-angle phase for a wall-to-wall jump.</summary>
    /// <param name="slot">Enemy supplying the departure position and jump span.</param>
    /// <param name="state">Per-enemy fields that retain the jump arc parameters.</param>
    /// <param name="jumpingRight">Selects the rightward endpoint and positive horizontal span when <see langword="true"/>.</param>
    private static void PrepareWallSpacePirateJump(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        bool jumpingRight)
    {
        int signedSpan = jumpingRight ? slot.Parameter2 : -slot.Parameter2;
        int signedHalfSpan = jumpingRight
            ? slot.Parameter2 >> 1
            : -(slot.Parameter2 >> 1);
        state.WallJumpDestinationX = unchecked((ushort)(slot.XPosition + signedSpan));
        state.WallJumpArcCenterX = unchecked((ushort)(slot.XPosition + signedHalfSpan));
        state.WallJumpArcCenterY = slot.YPosition;
        state.WallJumpArcAngle = jumpingRight ? (ushort)64 : (ushort)192;
    }

    /// <summary>Attempts laser allocation and records its count and sound request only when a projectile is created.</summary>
    /// <param name="slot">Wall Pirate supplying the laser origin.</param>
    /// <param name="state">Per-enemy diagnostic count incremented after successful allocation.</param>
    /// <param name="movingRight">Selects the laser's horizontal direction.</param>
    private void FireWallSpacePirateLaser(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state,
        bool movingRight)
    {
        // `$86:A009` queues $67 for every successful allocation. The later wall-jump opcode
        // independently queues $66; both are observable, distinct library-two requests.
        if (SpawnSpacePirateLaser(slot, movingRight, WallPirateLaserMuzzleYOffset))
        {
            state.SpawnedLaserCount++;
            LastSpacePirateSoundEffect = WallPirateLaserSound;
        }
    }

    /// <summary>
    /// Executes the nine wall-Pirate-specific instruction opcodes. Keeping family bytecode
    /// here prevents the common interpreter from becoming a second monolithic AI switch.
    /// </summary>
    private bool TryProcessWallSpacePirateInstruction(
        RoomEnemySlot slot,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor)
    {
        if (!IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer))
            return false;

        WallSpacePirateEnemyState state = RequireWallSpacePirateState(slot);
        switch (opcode)
        {
            case SpacePirateInstructionCodes.Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left:
                MoveWallSpacePirateAndReverseOnCollision(
                    slot,
                    state,
                    level,
                    ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2))),
                    onRightWall: false,
                    ref cursor);
                return true;

            case SpacePirateInstructionCodes.Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right:
                MoveWallSpacePirateAndReverseOnCollision(
                    slot,
                    state,
                    level,
                    ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2))),
                    onRightWall: true,
                    ref cursor);
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_RandomlyChooseADirection_LeftWall:
                RandomizeWallSpacePirateClimbDirection(state, onRightWall: false, ref cursor);
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_RandomlyChooseADirection_RightWall:
                RandomizeWallSpacePirateClimbDirection(state, onRightWall: true, ref cursor);
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_PrepareWallJumpToRight:
                PrepareWallSpacePirateJump(slot, state, jumpingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_PrepareWallJumpToLeft:
                PrepareWallSpacePirateJump(slot, state, jumpingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_FireLaserLeft:
                FireWallSpacePirateLaser(slot, state, movingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_FireLaserRight:
                FireWallSpacePirateLaser(slot, state, movingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_FunctionInY:
                state.Function = (WallSpacePirateFunction)ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case SpacePirateInstructionCodes.Instruction_PirateWall_QueueSpacePirateAttackSFX:
                LastSpacePirateSoundEffect = WallPirateJumpSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

    /// <summary>Returns the initialized wall-Pirate state associated with an enemy slot.</summary>
    /// <param name="slot">Slot whose per-enemy state was expected to be created during initialization.</param>
    /// <returns>The stored state view for that slot.</returns>
    /// <exception cref="InvalidOperationException">The slot has no initialized wall-Pirate state.</exception>
    private WallSpacePirateEnemyState RequireWallSpacePirateState(RoomEnemySlot slot) =>
        _wallSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized wall Space Pirate state.");
}
