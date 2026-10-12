namespace SuperMetroid.Core.Game;

/// <summary>The Zoa species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ZoaBehavior : IEnemySpecies
{
    /// <summary>Executes Zoa's private animation instructions.</summary>
    private bool TryProcessZoaInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Zoa ||
            !Enum.IsDefined((ZoaInstruction)word))
            return false;

        switch ((ZoaInstruction)word)
        {
            case ZoaInstruction.SetXSpeedTableIndexTo4:
                RequireZoaState(slot).XSpeedTableIndex = 4;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case ZoaInstruction.SetXSpeedTableIndexTo8:
                RequireZoaState(slot).XSpeedTableIndex = 8;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case ZoaInstruction.SetXSpeedTableIndexToC:
                RequireZoaState(slot).XSpeedTableIndex = 12;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Zoa does not own instruction ${word:X4}.");
        }
    }


    private const int ZoaActivationColumnDistance = 0x0080;
    private const int ZoaRisingSubpixelSpeed = 0x00008000;

    internal readonly ZoaEnemyState?[] _zoaStates = new ZoaEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Zoa</c> at $A3:B44A.</summary>
    internal void InitializeZoa(RoomEnemySlot slot)
    {
        var state = new ZoaEnemyState(slot)
        {
            Function = ZoaEnemyFunction.WaitForSamus,
            InstructionListTableIndex = ZoaAnimationSelector.None,
            PreviousInstructionListTableIndex = ZoaAnimationSelector.None,
            XSpeedTableIndex = 0,
            SpawnPosition = slot.Position.Whole,
        };
        _zoaStates[slot.SlotIndex] = state;

        // The initializer installs list zero directly while also setting “previous” to
        // zero. Consequently the later change detector is deliberately a no-op at rest.
        slot.CurrentInstruction = ZoaAnimationDefinitions.InstructionList(
            ZoaAnimationSelector.None);
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Ports <c>MainAI_Zoa</c> and its complete three-entry indirect dispatch.</summary>
    private static void RunZoaMain(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        if (samus is null)
            throw new InvalidOperationException("Zoa AI requires the active Samus actor.");

        switch (state.Function)
        {
            case ZoaEnemyFunction.WaitForSamus:
                RunZoaWait(slot, state, samus);
                return;
            case ZoaEnemyFunction.Rising:
                RunZoaRising(slot, state, samus);
                return;
            case ZoaEnemyFunction.Shooting:
                RunZoaShooting(slot, state, camera);
                return;
            default:
                throw new InvalidDataException(
                    $"Zoa function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    internal static void RunZoaWait(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        ushort signedDistance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        ushort absoluteDistance = unchecked((short)signedDistance) < 0
            ? unchecked((ushort)-signedDistance)
            : signedDistance;
        if (absoluteDistance >= ZoaActivationColumnDistance)
            return;

        // Odd indexes are rising lists: one faces left and three faces right. The sign test
        // uses the same wrapped Samus-minus-enemy word as the common bank-$A0 helper.
        state.InstructionListTableIndex = ZoaAnimationSelector.Rising |
            (unchecked((short)signedDistance) < 0
                ? ZoaAnimationSelector.None
                : ZoaAnimationSelector.FacingRight);
        SetZoaInstructionList(slot, state);
        state.Function = ZoaEnemyFunction.Rising;
    }

    internal static void RunZoaRising(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        if (unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)) < 0)
        {
            // NTSC subtracts exactly 0.5 px/frame. The PAL regional constant is 0.625;
            // this project targets the user's Japan/USA retail ROM and reads NTSC data.
            slot.Position = slot.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(-ZoaRisingSubpixelSpeed));
            return;
        }

        state.InstructionListTableIndex &= ZoaAnimationSelector.FacingRight;
        SetZoaInstructionList(slot, state);
        slot.VariableE = 0; // Native Enemy.var5; shared instruction-loop scratch word.
        state.Function = ZoaEnemyFunction.Shooting;
    }

    internal static void RunZoaShooting(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        Position16<RoomPixels> camera)
    {
        int unsignedDisplacement = ZoaSpeedDefinitions.Displacement(state.XSpeedTableIndex);

        // Shooting list zero travels left by subtraction; list two travels right by
        // addition. The instruction bytecode changes speed at 64-, 8-, and 48-frame marks.
        int xDisplacement = state.InstructionListTableIndex == ZoaAnimationSelector.None
            ? -unsignedDisplacement
            : unsignedDisplacement;
        slot.Position = slot.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(xDisplacement));
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
        {
            SetZoaInstructionList(slot, state);
            return;
        }

        // Once its center leaves the inclusive 256x256 camera square, Zoa teleports to the
        // saved population coordinate and becomes invisible until the next proximity wake.
        // `$A3:B51E-$B527` stores only the whole words: the subpixel fractions left by the
        // rise and the shot carry into the next flight.
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
        state.InstructionListTableIndex = ZoaAnimationSelector.None;
        SetZoaInstructionList(slot, state);
        state.Function = ZoaEnemyFunction.WaitForSamus;
    }

    internal static void SetZoaInstructionList(RoomEnemySlot slot, ZoaEnemyState state)
    {
        if (state.InstructionListTableIndex == state.PreviousInstructionListTableIndex)
            return;
        state.PreviousInstructionListTableIndex = state.InstructionListTableIndex;
        slot.CurrentInstruction = ZoaAnimationDefinitions.InstructionList(
            state.InstructionListTableIndex);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private ZoaEnemyState RequireZoaState(RoomEnemySlot slot) =>
        _zoaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Zoa state.");

    internal void RegisterZoaInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Zoa, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Zoa, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeZoa(slot);
        });
    }

    internal void RegisterZoaMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Zoa, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Zoa, (slot, frame, nmiFrameCounter8) =>
        {
            RunZoaMain(slot, RequireZoaState(slot), frame.Samus, frame.Camera);
        });
    }

    internal void RegisterZoaOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessZoaInstruction(slot, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_zoaStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
