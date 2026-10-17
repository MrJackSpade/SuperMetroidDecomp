using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The Ripper species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class RipperBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.GRipper, EnemyDefinitionId.Ripper, EnemyDefinitionId.Ripper2];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new RipperEntity(this, slot, common);

    /// <summary>Resolves horizontal wall collisions for all three Ripper variants.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Shares room collision behavior across ordinary Ripper, GRipper, and Ripper II AI.</summary>
    internal RipperBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>The words each physical slot keeps across Ripper occupants for the current room.</summary>
    internal readonly RipperExtensionWords[] _extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static RipperExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, RoomEnemySystem.MaximumEnemyCount).Select(_ => new RipperExtensionWords())];

    /// <summary>Clears variant patrol endpoints, AI state and loops before loading another room.</summary>
    internal void ResetRipperVariantRoomState()
    {
        for (int slotIndex = 0; slotIndex < _extensions.Length; slotIndex++)
            _extensions[slotIndex] = new RipperExtensionWords();
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
        bool selectsPositiveVelocity = (packedSelector & RipperMasks.VelocitySelectorBits) != 0;
        LoadRipperVariantVelocity(state, selectsPositiveVelocity);
        SetRipperProgram(
            slot,
            state.XVelocity.WholePixels < 0 ? RipperProgram.GRipperMovingLeft : RipperProgram.GRipperMovingRight);

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
        int displacement = state.XVelocity.RawValue;
        bool reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            displacement);
        if (!reverse && state.XVelocity.WholePixels < 0)
        {
            reverse = unchecked((short)(slot.Position.X.Whole - state.MinimumXPosition)) < 0;
        }
        if (!reverse && state.XVelocity.WholePixels >= 0)
        {
            reverse = unchecked((short)(slot.Position.X.Whole - state.MaximumXPosition)) >= 0;
        }
        if (reverse)
        {
            ReverseRipperVariant(
                slot,
                positiveProgram: RipperProgram.GRipperMovingRight,
                negativeProgram: RipperProgram.GRipperMovingLeft);
        }
    }

    /// <summary>Ports <c>InitAI_Ripper2</c> at $A2:E318.</summary>
    internal void InitializeRipper2(RoomEnemySlot slot)
    {
        RipperVariantEnemyState state = CreateRipperVariantState(slot);
        state.SpeedTableByteOffset = unchecked((ushort)(slot.Parameter1 * 8));

        // Ripper II's list labels appear inverted relative to the displayed shared maps.
        // Keep the native list names instead of normalizing them: init1 zero uses the
        // negative speed pair with the moving-right list, while nonzero uses positive speed
        // with the moving-left list.
        bool movingPositive = slot.Parameter2 != 0;
        LoadRipperVariantVelocity(state, movingPositive);
        SetRipperProgram(slot, movingPositive ? RipperProgram.Ripper2MovingLeft : RipperProgram.Ripper2MovingRight);
    }

    /// <summary>Ports <c>MainAI_Ripper2</c> at $A2:E353.</summary>
    private void RunRipper2Main(RoomEnemySlot slot, RipperVariantEnemyState state, RoomLevelData? level)
    {
        RequireRipperTerrain(level, "Ripper II");
        int displacement = state.XVelocity.RawValue;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        ReverseRipperVariant(
            slot,
            positiveProgram: RipperProgram.Ripper2MovingLeft,
            negativeProgram: RipperProgram.Ripper2MovingRight);
    }

    /// <summary>Ports <c>Ripper_Init</c> at $A2:E49F.</summary>
    internal void InitializeRipper(RoomEnemySlot slot)
    {
        // Population parameter two is the initial facing/direction selector. A nonzero word
        // chooses the positive table pair and the right-facing list; zero chooses the
        // negative pair and left-facing list. Unlike a reversal, initialization keeps the
        // population's instruction timer.
        bool movingRight = slot.Parameter2 != 0;
        _extensions[slot.SlotIndex].Program = AnimationProgramCursor<RipperProgram>.Start(
            movingRight ? RipperProgram.RipperMovingRight : RipperProgram.RipperMovingLeft);

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
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        bool wasMovingLeft = (short)slot.VariableD < 0;
        bool nowMovingRight = wasMovingLeft;
        LoadRipperVelocity(slot, nowMovingRight);
        SetRipperProgram(slot, nowMovingRight ? RipperProgram.RipperMovingRight : RipperProgram.RipperMovingLeft);
    }

    /// <summary>Loads a GRipper's or Ripper II's velocity from its shared eight-byte speed record.</summary>
    /// <param name="state">The variant's state, whose speed record is read.</param>
    /// <param name="movingRight">Whether the record's positive half is loaded rather than its negative half.</param>
    internal static void LoadRipperVariantVelocity(RipperVariantEnemyState state, bool movingRight)
    {
        (short whole, ushort fraction) = EnemyLinearSpeedDefinitions.Read(state.SpeedTableByteOffset + (movingRight ? 0 : 4));
        state.XVelocity = SnesSignedSixteenSixteen.FromParts(whole, fraction);
    }

    /// <summary>Loads the signed whole and fractional velocity pair from the shared eight-byte speed record.</summary>
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
        if (slot.FrozenTimer.IsZero)
            return;
        slot.SpritemapPointer = RequireRipperVariantState(slot).XVelocity.WholePixels < 0
            ? RipperFrameDefinitions.FrozenFacingLeftSpritemap
            : RipperFrameDefinitions.FrozenFacingRightSpritemap;
    }

    /// <summary>Binds a variant slot to its per-room patrol arrays and retains its AI state.</summary>
    private RipperVariantEnemyState CreateRipperVariantState(RoomEnemySlot slot)
    {
        var state = new RipperVariantEnemyState(_extensions[slot.SlotIndex]);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        return state;
    }

    /// <summary>Rejects movement outside a loaded collision allocation.</summary>
    private static void RequireRipperTerrain([NotNull] RoomLevelData? level, string species)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                $"{species} movement requires the active room collision allocation.");
        }
    }

    /// <summary>Flips a GRipper or Ripper II speed pair and restarts the corresponding loop.</summary>
    internal void ReverseRipperVariant(
        RoomEnemySlot slot,
        RipperProgram positiveProgram,
        RipperProgram negativeProgram)
    {
        RipperVariantEnemyState state = RequireRipperVariantState(slot);
        bool wasMovingNegative = state.XVelocity.WholePixels < 0;
        LoadRipperVariantVelocity(state, movingRight: wasMovingNegative);
        SetRipperProgram(slot, wasMovingNegative ? positiveProgram : negativeProgram);
    }

    /// <summary>Installs a flying loop from its first step on the next enemy frame.</summary>
    private void SetRipperProgram(RoomEnemySlot slot, RipperProgram program)
    {
        _extensions[slot.SlotIndex].Program = AnimationProgramCursor<RipperProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Advances Ripper's typed animation.</summary>
    internal void AdvanceRipperTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<RipperProgram> cursor = _extensions[slot.SlotIndex].Program;
        EnemyProgramAnimation.Run(slot, RipperAnimations.Programs, ref cursor, RipperFrameDefinitions.Spritemap);
        _extensions[slot.SlotIndex].Program = cursor;
    }

    /// <summary>Requires variant initialization before dispatching GRipper or Ripper II main AI.</summary>
    private static RipperVariantEnemyState RequireRipperVariantState(RoomEnemySlot slot) =>
        slot.SpeciesState<RipperVariantEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized GRipper/Ripper II state.");

    /// <summary>Preserves the direction-specific frozen map after common bomb reactions.</summary>
    internal static void RegisterRipperBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.GRipperRipper2Shot)
        {
            AfterCommon = c => ResolveGRipperRipper2ShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Ports GRipper's initialization AI.</summary>
    internal void RunGRipperInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeGRipper(slot);
    }

    /// <summary>Ports Ripper2's initialization AI.</summary>
    internal void RunRipper2InitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeRipper2(slot);
    }

    /// <summary>Ports Ripper's initialization AI.</summary>
    internal void RunRipperInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeRipper(slot);
    }

    /// <summary>Ports GRipper's main AI.</summary>
    internal void RunGRipperMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGRipperMain(slot, RequireRipperVariantState(slot), frame.Level);
    }

    /// <summary>Ports Ripper2's main AI.</summary>
    internal void RunRipper2MainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunRipper2Main(slot, RequireRipperVariantState(slot), frame.Level);
    }

    /// <summary>Ports Ripper's main AI.</summary>
    internal void RunRipperMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunRipperMain(slot, frame.Level);
    }

    /// <summary>Pins GRipper and Ripper II frozen visuals after accepted shot reactions.</summary>
    internal static void RegisterRipperShotReactions(EnemySharedReactions table)
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
}
