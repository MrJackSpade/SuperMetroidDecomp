using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Ripper species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class RipperBehavior : IEnemySpecies
{
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Initializes a new ripper behavior instance.</summary>
    internal RipperBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }


    /// <summary>Stores ripper variant minimum xpositions.</summary>
    private readonly ushort[] _ripperVariantMinimumXPositions =
        new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores ripper variant maximum xpositions.</summary>
    private readonly ushort[] _ripperVariantMaximumXPositions =
        new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores ripper variant states.</summary>
    private readonly RipperVariantEnemyState?[] _ripperVariantStates =
        new RipperVariantEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>reset ripper variant room state.</summary>
    internal void ResetRipperVariantRoomState()
    {
        Array.Clear(_ripperVariantMinimumXPositions);
        Array.Clear(_ripperVariantMaximumXPositions);
        Array.Clear(_ripperVariantStates);
    }

    /// <summary>Ports <c>InitAI_GRipper</c> at $A2:E1D3.</summary>
    internal void InitializeGRipper(RoomEnemySlot slot)
    {
        var state = CreateRipperVariantState(slot);

        // GRipper overloads the population's initialization-list word as a packed speed
        // selector. Only its low byte reaches the eight-byte speed-table multiplication.
        // The peculiar BIT #$FEFF direction test is preserved literally: every retail word
        // with a nonzero speed field selects the positive pair, even when bit eight is set.
        ushort packedSelector = slot.CurrentInstruction;
        state.SpeedTableByteOffset = unchecked((ushort)((SnesLittleEndian.LowByte(packedSelector)) * 8));
        bool selectsPositiveVelocity = (packedSelector & 0xfeff) != 0;
        LoadRipperVelocity(slot, selectsPositiveVelocity);
        SetRipperInstructionList(
            slot,
            unchecked((short)state.XVelocity) < 0
                ? RipperInstructionProgramDefinitions.GRipperMovingLeft
                : RipperInstructionProgramDefinitions.GRipperMovingRight);

        // Unlike the wall-collision helper, patrol-bound reversal does not clamp the actor
        // back to either endpoint. The one-frame fractional overshoot is native and becomes
        // visible again when the direction reverses on the next pass.
        state.MinimumXPosition = slot.Parameter1;
        state.MaximumXPosition = slot.Parameter2;
    }

    /// <summary>Ports <c>MainAI_GRipper</c> at $A2:E221.</summary>
    private void RunGRipperMain(
        RoomEnemySlot slot,
        RipperVariantEnemyState state,
        RoomLevelData? level)
    {
        RequireRipperTerrain(level, "GRipper");
        int displacement = unchecked((SnesSixteenSixteen.FromWhole((short)state.XVelocity)) | state.XSubvelocity);
        bool reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level!,
            slot,
            displacement);
        if (!reverse && unchecked((short)state.XVelocity) < 0)
        {
            reverse = unchecked((short)(slot.Position.X.Whole - state.MinimumXPosition)) < 0;
        }
        if (!reverse && unchecked((short)state.XVelocity) >= 0)
        {
            reverse = unchecked((short)(slot.Position.X.Whole - state.MaximumXPosition)) >= 0;
        }
        if (reverse)
        {
            ReverseRipperVariant(
                slot,
                positiveInstruction: RipperInstructionProgramDefinitions.GRipperMovingRight,
                negativeInstruction: RipperInstructionProgramDefinitions.GRipperMovingLeft);
        }
    }

    /// <summary>Ports <c>InitAI_Ripper2</c> at $A2:E318.</summary>
    internal void InitializeRipper2(RoomEnemySlot slot)
    {
        RipperVariantEnemyState state = CreateRipperVariantState(slot);
        state.SpeedTableByteOffset = unchecked((ushort)(slot.Parameter1 * 8));

        // Ripper II's list labels appear inverted relative to the displayed shared maps.
        // Keep the literal ROM pointers instead of normalizing their names: init1 zero uses
        // the negative speed pair with $E2E0, while nonzero uses positive speed with $E2F4.
        bool movingPositive = slot.Parameter2 != 0;
        LoadRipperVelocity(slot, movingPositive);
        SetRipperInstructionList(
            slot,
            movingPositive
                ? RipperInstructionProgramDefinitions.Ripper2MovingLeft
                : RipperInstructionProgramDefinitions.Ripper2MovingRight);
    }

    /// <summary>Ports <c>MainAI_Ripper2</c> at $A2:E353.</summary>
    private void RunRipper2Main(RoomEnemySlot slot, RoomLevelData? level)
    {
        RequireRipperTerrain(level, "Ripper II");
        int displacement = unchecked((int)SnesSixteenSixteen.Compose(slot.VariableD, slot.VariableC));
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level!, slot, displacement))
            return;

        ReverseRipperVariant(
            slot,
            positiveInstruction: RipperInstructionProgramDefinitions.Ripper2MovingLeft,
            negativeInstruction: RipperInstructionProgramDefinitions.Ripper2MovingRight);
    }

    /// <summary>Ports <c>Ripper_Init</c> at $A2:E49F.</summary>
    internal static void InitializeRipper(RoomEnemySlot slot)
    {
        // Population parameter two is the initial facing/direction selector. A nonzero word
        // chooses the positive table pair and the right-facing list; zero chooses the
        // negative pair and left-facing list.
        bool movingRight = slot.Parameter2 != 0;
        slot.CurrentInstruction = movingRight
            ? RipperInstructionProgramDefinitions.RipperMovingRight
            : RipperInstructionProgramDefinitions.RipperMovingLeft;

        // Each logical speed occupies eight bytes in the common table:
        //   +0 signed pixel velocity, +2 subpixel velocity,
        //   +4 negated pixel velocity, +6 negated subpixel velocity.
        // Variable E retains the byte offset because the reversal routine reuses it.
        slot.VariableE = unchecked((ushort)(slot.Parameter1 * 8));
        LoadRipperVelocity(slot, movingRight);
    }

    /// <summary>Ports <c>Ripper_Main</c> at $A2:E4DA.</summary>
    private void RunRipperMain(RoomEnemySlot slot, RoomLevelData? level)
    {
        RequireRipperTerrain(level, "Ripper");

        // $A0:C6AB consumes a signed 16.16 displacement whose high and low words are kept
        // in Variables D/C. On collision the native helper aligns the actor flush with the
        // wall before Ripper swaps direction and animation.
        int displacement = unchecked((int)SnesSixteenSixteen.Compose(slot.VariableD, slot.VariableC));
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level!, slot, displacement))
            return;

        bool wasMovingLeft = (short)slot.VariableD < 0;
        bool nowMovingRight = wasMovingLeft;
        LoadRipperVelocity(slot, nowMovingRight);
        slot.CurrentInstruction = nowMovingRight
            ? RipperInstructionProgramDefinitions.RipperMovingRight
            : RipperInstructionProgramDefinitions.RipperMovingLeft;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>load ripper velocity.</summary>
    internal static void LoadRipperVelocity(RoomEnemySlot slot, bool movingRight)
    {
        var velocity = EnemyLinearSpeedDefinitions.Read(slot.VariableE + (movingRight ? 0 : 4));
        slot.VariableD = unchecked((ushort)velocity.Whole);
        slot.VariableC = velocity.Fraction;
    }

    /// <summary>
    /// Ports the common post-shot tail at $A2:E3AD. Normal shot AI owns health, death,
    /// impact, and freeze timers; this family-specific tail only pins the direction-specific
    /// two-piece frozen map when the accepted projectile actually froze the actor.
    /// </summary>
    private static void ResolveGRipperRipper2ShotAfterCommon(RoomEnemySlot slot)
    {
        if (slot.FrozenTimer == 0)
            return;
        slot.SpritemapPointer = unchecked((short)slot.VariableD) < 0
            ? RipperInstructionProgramDefinitions.FrozenFacingLeftSpritemap
            : RipperInstructionProgramDefinitions.FrozenFacingRightSpritemap;
    }

    /// <summary>create ripper variant state.</summary>
    private RipperVariantEnemyState CreateRipperVariantState(RoomEnemySlot slot)
    {
        var state = new RipperVariantEnemyState(
            slot,
            _ripperVariantMinimumXPositions,
            _ripperVariantMaximumXPositions);
        _ripperVariantStates[slot.SlotIndex] = state;
        return state;
    }

    /// <summary>require ripper terrain.</summary>
    private static void RequireRipperTerrain(RoomLevelData? level, string species)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                $"{species} movement requires the active room collision allocation.");
        }
    }

    /// <summary>reverse ripper variant.</summary>
    internal static void ReverseRipperVariant(
        RoomEnemySlot slot,
        ushort positiveInstruction,
        ushort negativeInstruction)
    {
        bool wasMovingNegative = unchecked((short)slot.VariableD) < 0;
        LoadRipperVelocity(slot, movingRight: wasMovingNegative);
        SetRipperInstructionList(
            slot,
            wasMovingNegative ? positiveInstruction : negativeInstruction);
    }

    /// <summary>set ripper instruction list.</summary>
    private static void SetRipperInstructionList(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>require ripper variant state.</summary>
    private RipperVariantEnemyState RequireRipperVariantState(RoomEnemySlot slot) =>
        _ripperVariantStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized GRipper/Ripper II state.");

    /// <summary>register ripper bomb reactions.</summary>
    internal static void RegisterRipperBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.GRipperRipper2Shot)
        {
            AfterCommon = c => ResolveGRipperRipper2ShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>register ripper initialization ai.</summary>
    internal void RegisterRipperInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_GRipper, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.GRipper, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeGRipper(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Ripper2, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper2, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeRipper2(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Ripper, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeRipper(slot);
        });
    }

    /// <summary>register ripper main ai.</summary>
    internal void RegisterRipperMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_GRipper, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.GRipper, (slot, frame, nmiFrameCounter8) =>
        {
            RunGRipperMain(slot, RequireRipperVariantState(slot), frame.Level);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_Ripper2, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper2, (slot, frame, nmiFrameCounter8) =>
        {
            RunRipper2Main(slot, frame.Level);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_Ripper, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper, (slot, frame, nmiFrameCounter8) =>
        {
            RunRipperMain(slot, frame.Level);
        });
    }

    /// <summary>register ripper shot reactions.</summary>
    internal static void RegisterRipperShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.GRipperRipper2Shot)
        {
            AfterCommon = (c, _) => ResolveGRipperRipper2ShotAfterCommon(c.Enemy),
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetRipperVariantRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
