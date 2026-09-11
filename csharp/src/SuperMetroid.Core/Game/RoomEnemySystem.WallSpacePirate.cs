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
    None = 0,
    StartsOnRightWall = 0x0001,
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
    Down = 0,
    Up = 1,
}

/// <summary>
/// Debugger-facing view of the native wall Pirate work words. Variables A-F remain backed
/// directly by the enemy slot, while the three bank-$7E:7800 scratch arrays are retained as
/// explicit host fields because they do not have ordinary per-slot variable aliases.
/// </summary>
public sealed class WallSpacePirateEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal WallSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

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

    public WallSpacePirateClimbDirection ClimbDirection
    {
        get => (WallSpacePirateClimbDirection)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    public ushort WallJumpArcCenterX
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort WallJumpArcCenterY
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort WallJumpArcAngle
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort RightJumpTargetAngle { get; internal set; }
    public ushort LeftJumpTargetAngle { get; internal set; }
    public ushort JumpAngleDelta { get; internal set; }
    public int SpawnedLaserCount { get; internal set; }
}

/// <summary>
/// Translation of wall Space Pirates $F353/$F393/$F3D3/$F413/$F453/$F493. This covers the
/// complete retail family: wall climbing, solid collision, random reversals, firing, both
/// wall-jump arcs, animation bytecode, and the shared Pirate/Mother-Brain laser.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort GreyWallSpacePirateDefinition = 0xf353;
    internal const ushort GreenWallSpacePirateDefinition = 0xf393;
    internal const ushort RedWallSpacePirateDefinition = 0xf3d3;
    internal const ushort GoldWallSpacePirateDefinition = 0xf413;
    internal const ushort MagentaWallSpacePirateDefinition = 0xf453;
    internal const ushort SilverWallSpacePirateDefinition = 0xf493;

    private const ushort WallPirateFireAndJumpLeft = 0xecc0;
    private const ushort WallPirateLandedOnLeftWall = 0xece4;
    private const ushort WallPirateMovingUpLeftWall = 0xecec;
    private const ushort WallPirateMovingDownLeftWall = 0xed36;
    private const ushort WallPirateFireAndJumpRight = 0xed80;
    private const ushort WallPirateLandedOnRightWall = 0xeda4;
    private const ushort WallPirateMovingDownRightWall = 0xedac;
    private const ushort WallPirateMovingUpRightWall = 0xedf6;

    private const int WallPirateSamusDetectionBand = 32;
    private const int WallPirateLaserMuzzleYOffset = 16;
    private const ushort WallPirateLaserSound = 0x0067;
    private const ushort WallPirateJumpSound = 0x0066;

    private readonly WallSpacePirateEnemyState?[] _wallSpacePirateStates =
        new WallSpacePirateEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<WallSpacePirateEnemyState?> WallSpacePirateStates =>
        _wallSpacePirateStates;

    internal static bool IsWallSpacePirateDefinition(ushort definition) => definition is
        GreyWallSpacePirateDefinition or
        GreenWallSpacePirateDefinition or
        RedWallSpacePirateDefinition or
        GoldWallSpacePirateDefinition or
        MagentaWallSpacePirateDefinition or
        SilverWallSpacePirateDefinition;

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
            ? WallPirateMovingDownRightWall
            : WallPirateMovingDownLeftWall;
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
                    InstallWallSpacePirateInstruction(slot, WallPirateFireAndJumpRight);
                return;

            case WallSpacePirateFunction.ClimbingRightWall:
                if (WallSpacePirateCanAttack(slot, RequireSamus()))
                    InstallWallSpacePirateInstruction(slot, WallPirateFireAndJumpLeft);
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

    private static void StepWallSpacePirateJumpRight(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        PositionWallSpacePirateOnArc(slot, state);
        state.WallJumpArcAngle = unchecked((byte)(
            state.WallJumpArcAngle - state.JumpAngleDelta));
        if (state.WallJumpArcAngle != state.RightJumpTargetAngle)
            return;

        InstallWallSpacePirateInstruction(slot, WallPirateLandedOnRightWall);
        SnapWallSpacePirateXToTile(slot);
    }

    private static void StepWallSpacePirateJumpLeft(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        PositionWallSpacePirateOnArc(slot, state);
        state.WallJumpArcAngle = unchecked((byte)(
            state.WallJumpArcAngle + state.JumpAngleDelta));
        if (state.WallJumpArcAngle != state.LeftJumpTargetAngle)
            return;

        InstallWallSpacePirateInstruction(slot, WallPirateLandedOnLeftWall);
        SnapWallSpacePirateXToTile(slot);
    }

    private static void PositionWallSpacePirateOnArc(
        RoomEnemySlot slot,
        WallSpacePirateEnemyState state)
    {
        // Native math discards subpixels and rewrites both positions every frame. The jump
        // ellipse is parameter2 pixels wide and parameter2/4 pixels tall.
        slot.XPosition = unchecked((ushort)(
            state.WallJumpArcCenterX +
            ReadEightBitNegativeSineProduct(
                state.WallJumpArcAngle,
                (ushort)(slot.Parameter2 >> 1))));
        slot.YPosition = unchecked((ushort)(
            state.WallJumpArcCenterY -
            ReadEightBitCosineProduct(state.WallJumpArcAngle, (ushort)(slot.Parameter2 >> 2))));
        slot.XSubposition = 0;
        slot.YSubposition = 0;
    }

    private static bool WallSpacePirateCanAttack(RoomEnemySlot slot, SamusState samus) =>
        Math.Abs(unchecked((short)(samus.YPosition - slot.YPosition))) <
            WallPirateSamusDetectionBand;

    private static void SnapWallSpacePirateXToTile(RoomEnemySlot slot)
    {
        // `$B2:EFFF` deliberately has two alignment grids. Nibbles 0..10 round down to an
        // eight-pixel boundary; 11..15 round up to the next sixteen-pixel boundary.
        slot.XPosition = (slot.XPosition & 0x000f) < 11
            ? unchecked((ushort)(slot.XPosition & 0xfff8))
            : unchecked((ushort)((slot.XPosition & 0xfff0) + 16));
        slot.XSubposition = 0;
    }

    private static void InstallWallSpacePirateInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

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

    private static ushort SelectWallSpacePirateClimbList(
        bool onRightWall,
        WallSpacePirateClimbDirection direction) => (onRightWall, direction) switch
    {
        (false, WallSpacePirateClimbDirection.Down) => WallPirateMovingDownLeftWall,
        (false, WallSpacePirateClimbDirection.Up) => WallPirateMovingUpLeftWall,
        (true, WallSpacePirateClimbDirection.Down) => WallPirateMovingDownRightWall,
        (true, WallSpacePirateClimbDirection.Up) => WallPirateMovingUpRightWall,
        _ => throw new InvalidOperationException(
            $"Wall Pirate climb direction ${(ushort)direction} is not binary."),
    };

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
        int operandAddress = (slot.Definition.Bank << 16) |
            unchecked((ushort)(cursor + 2));
        switch (opcode)
        {
            case SpacePirateInstructionCodes.Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left:
                MoveWallSpacePirateAndReverseOnCollision(
                    slot,
                    state,
                    level,
                    ReadWord(_bus!, operandAddress),
                    onRightWall: false,
                    ref cursor);
                return true;

            case SpacePirateInstructionCodes.Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right:
                MoveWallSpacePirateAndReverseOnCollision(
                    slot,
                    state,
                    level,
                    ReadWord(_bus!, operandAddress),
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
                state.Function = (WallSpacePirateFunction)ReadWord(_bus!, operandAddress);
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

    private WallSpacePirateEnemyState RequireWallSpacePirateState(RoomEnemySlot slot) =>
        _wallSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized wall Space Pirate state.");
}
