using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Skultera species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SkulteraBehavior : IEnemySpecies
{
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Initializes a new skultera behavior instance.</summary>
    internal SkulteraBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Executes Skultera's private animation instructions.</summary>
    private bool TryProcessSkulteraInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Skultera ||
            !Enum.IsDefined((SkulteraInstruction)word))
            return false;

        switch ((SkulteraInstruction)word)
        {
            case SkulteraInstruction.SetLayerTo6:
                // The right-facing steady list promotes the fish above foreground
                // scenery only after its first instruction tick, exactly like the ROM.
                slot.Layer = 6;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case SkulteraInstruction.SetLayerTo2:
                // The left-facing steady list draws below the matching scenery layer.
                slot.Layer = 2;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case SkulteraInstruction.SetTurnFinishedFlag:
                // Turning lists sleep immediately after publishing this flag. Main AI
                // consumes it on the following frame and installs steady facing art.
                RequireSkulteraState(slot).TurnFinished = true;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Skultera does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Stores skultera radii.</summary>
    internal readonly ushort[] _skulteraRadii = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores skultera turn finished flags.</summary>
    internal readonly ushort[] _skulteraTurnFinishedFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores skultera angle deltas.</summary>
    internal readonly ushort[] _skulteraAngleDeltas = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores skultera previous yoffsets.</summary>
    internal readonly ushort[] _skulteraPreviousYOffsets = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores skultera current yoffsets.</summary>
    internal readonly ushort[] _skulteraCurrentYOffsets = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores skultera states.</summary>
    internal readonly SkulteraEnemyState?[] _skulteraStates =
        new SkulteraEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Skultera</c> at <c>$A3:90B5</c>.</summary>
    internal void InitializeSkultera(RoomEnemySlot slot)
    {
        var state = new SkulteraEnemyState(
            slot,
            _skulteraRadii,
            _skulteraTurnFinishedFlags,
            _skulteraAngleDeltas,
            _skulteraPreviousYOffsets,
            _skulteraCurrentYOffsets);
        _skulteraStates[slot.SlotIndex] = state;

        // Parameter one's high byte selects initial facing: zero means right, any nonzero
        // value means left. Its low byte is multiplied by eight because each common speed
        // magnitude stores adjacent {right whole, right fraction, left whole, left fraction}
        // words. Keep the byte offset visible; treating it as an array index is a common
        // source of subtly wrong half-pixel speeds in ports of this AI.
        bool startsLeft = (SnesLittleEndian.HighByteInPlace(slot.Parameter1)) != 0;
        slot.CurrentInstruction = startsLeft
            ? SkulteraInstructionProgramDefinitions.SwimmingLeft
            : SkulteraInstructionProgramDefinitions.SwimmingRight;
        state.Function = startsLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;

        ushort speedTableOffset = unchecked((ushort)((SnesLittleEndian.LowByte(slot.Parameter1)) * 8));
        (state.RightVelocity, state.RightSubvelocity) =
            EnemyMath.ReadLinearEnemySpeed(speedTableOffset);
        (state.LeftVelocity, state.LeftSubvelocity) =
            EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));

        // Parameter two packs radius in its low byte and unsigned initial angle delta in
        // its high byte. The delta becomes a signed word only when a turn negates it.
        state.Radius = unchecked((ushort)(SnesLittleEndian.LowByte(slot.Parameter2)));
        state.AngleDelta = (short)SnesLittleEndian.HighByte(slot.Parameter2);
        state.Angle = 0;
        state.TurnFinished = false;
        state.CurrentYOffset = 0;
        state.PreviousYOffset = unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(
            state.Angle,
            state.Radius));
    }

    /// <summary>Ports <c>MainAI_Skultera</c> at <c>$A3:912B</c>.</summary>
    private void RunSkulteraMain(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Skultera AI requires active room collision data.");

        switch (state.Function)
        {
            case SkulteraEnemyFunction.SwimmingLeft:
                SwimSkultera(slot, state, level, movingLeft: true);
                return;
            case SkulteraEnemyFunction.SwimmingRight:
                SwimSkultera(slot, state, level, movingLeft: false);
                return;
            case SkulteraEnemyFunction.TurningRight:
                FinishSkulteraTurn(slot, state, nowMovingLeft: false);
                return;
            case SkulteraEnemyFunction.TurningLeft:
                FinishSkulteraTurn(slot, state, nowMovingLeft: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Skultera function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the mirrored swimming functions at <c>$A3:9132/$91AB</c>.</summary>
    private void SwimSkultera(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData level,
        bool movingLeft)
    {
        int horizontalDisplacement = movingLeft
            ? state.LeftDisplacement
            : state.RightDisplacement;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: movingLeft);
            // Horizontal collision jumps directly to the native merge label. Consequently
            // CurrentYOffset remains the preceding frame's word and is copied below again.
            state.PreviousYOffset = state.CurrentYOffset;
            return;
        }

        state.CurrentYOffset = unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(
            state.Angle,
            state.Radius));
        short yDifference = unchecked((short)(
            state.CurrentYOffset - state.PreviousYOffset));
        if (_terrain.MoveEnemyVertically(level, slot, SnesSixteenSixteen.FromWhole(yDifference)))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: movingLeft);
        }
        else
        {
            // Only unobstructed movement advances the low-byte phase. The AND is not
            // cosmetic: native AI discards any high word produced by signed delta wrapping.
            state.Angle = unchecked((ushort)(unchecked((byte)(state.Angle + state.AngleDelta))));
        }

        // Both successful movement and vertical collision retain the newly calculated
        // offset. This prevents a failed vertical step from being retried after the turn.
        state.PreviousYOffset = state.CurrentYOffset;
    }

    /// <summary>Begins skultera turn.</summary>
    internal static void BeginSkulteraTurn(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        bool currentlyMovingLeft)
    {
        state.Function = currentlyMovingLeft
            ? SkulteraEnemyFunction.TurningRight
            : SkulteraEnemyFunction.TurningLeft;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = currentlyMovingLeft
            ? SkulteraInstructionProgramDefinitions.TurningRight
            : SkulteraInstructionProgramDefinitions.TurningLeft;
    }

    /// <summary>Ports the mirrored turn completion functions at <c>$A3:9224/$9256</c>.</summary>
    private static void FinishSkulteraTurn(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        bool nowMovingLeft)
    {
        if (!state.TurnFinished)
            return;

        state.TurnFinished = false;
        state.Function = nowMovingLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;
        state.AngleDelta = unchecked((short)-state.AngleDelta);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = nowMovingLeft
            ? SkulteraInstructionProgramDefinitions.SwimmingLeft
            : SkulteraInstructionProgramDefinitions.SwimmingRight;
    }

    /// <summary>Requires skultera state.</summary>
    private SkulteraEnemyState RequireSkulteraState(RoomEnemySlot slot) =>
        _skulteraStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Skultera state.");

    /// <summary>Registers skultera initialization ai.</summary>
    internal void RegisterSkulteraInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Skultera, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Skultera, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeSkultera(slot);
        });
    }

    /// <summary>Registers skultera main ai.</summary>
    internal void RegisterSkulteraMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Skultera, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Skultera, (slot, frame, nmiFrameCounter8) =>
        {
            RunSkulteraMain(slot, RequireSkulteraState(slot), frame.Level);
        });
    }

    /// <summary>Registers skultera owned instructions.</summary>
    internal void RegisterSkulteraOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessSkulteraInstruction(slot, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_skulteraStates);
        Array.Clear(_skulteraRadii);
        Array.Clear(_skulteraTurnFinishedFlags);
        Array.Clear(_skulteraAngleDeltas);
        Array.Clear(_skulteraPreviousYOffsets);
        Array.Clear(_skulteraCurrentYOffsets);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
