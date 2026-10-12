namespace SuperMetroid.Core.Game;

/// <summary>The Spark species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SparkBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;

    private readonly FallingSparksBehavior _fallingSparksBehavior;

    internal SparkBehavior(EnemyRoomBinding room, FallingSparksBehavior fallingSparksBehavior)
    {
        _fallingSparksBehavior = fallingSparksBehavior;
        _room = room;
    }

    /// <summary>Executes Spark's private animation instructions.</summary>
    private static bool TryProcessSparkInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Spark ||
            !Enum.IsDefined((SparkInstruction)word))
            return false;

        switch ((SparkInstruction)word)
        {
            case SparkInstruction.SetAsIntangible:
                // Spark flicker-out command: property bit $0400 removes the actor from
                // every ordinary Samus, beam, and grapple collision pass.
                slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case SparkInstruction.SetAsTangible:
                // Spark flicker-on command executes before the first visible activation
                // frame, so collision and art become live together.
                slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Spark does not own instruction ${word:X4}.");
        }
    }


    internal readonly SparkEnemyState?[] _sparkStates =
        new SparkEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Spark</c> at <c>$A8:E637</c>.</summary>
    internal void InitializeSpark(RoomEnemySlot slot)
    {
        var state = new SparkEnemyState(slot);
        _sparkStates[slot.SlotIndex] = state;

        // Native masks the selector to two bits, but both tables contain only three authored
        // words. The compiled definition preserves selector three's two adjacent-code reads;
        // a later main pass still reports its exact nonsensical function if selected.
        SparkMovementDefinition definition = SparkMovementDefinitions.InitialState(slot.Parameter1);
        state.Function = definition.Function;
        state.BaseFunctionTime = slot.Parameter2;
        SetSparkFunctionTimer(state, additionalTime: 0);

        slot.InstructionTimer = 1;
        slot.CurrentInstruction = definition.InstructionList;
        slot.Timer = 0;

        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
            return;

        // This is intentionally *not* `ORA #$0100`. The retail instruction is the odd
        // absolute-memory form `ORA $0100`, noted as a bug in the disassembly. Preserve the
        // exact live WRAM dependency so stack-page contents, including a zero word, produce
        // the same properties as the cartridge instead of inventing forced invisibility.
        slot.Properties = unchecked((ushort)(
            slot.Properties | new EnemyOverreadMemory(_room.MemoryBus).SparkPropertyOverlay));
    }

    /// <summary>Ports the indirect main dispatcher at <c>$A8:E68E</c>.</summary>
    private void RunSparkMain(RoomEnemySlot slot, SparkEnemyState state)
    {
        switch (state.Function)
        {
            case SparkEnemyFunction.AlwaysActive:
                return;

            case SparkEnemyFunction.IntermittentInactive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Function = SparkEnemyFunction.IntermittentActive;
                InstallSparkInstructionList(slot, SparkInstructionProgramDefinitions.FlickerOn);
                SetSparkFunctionTimer(state, additionalTime: 0);
                return;

            case SparkEnemyFunction.IntermittentActive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Function = SparkEnemyFunction.IntermittentInactive;
                InstallSparkInstructionList(slot, SparkInstructionProgramDefinitions.FlickerOut);
                SetSparkFunctionTimer(state, additionalTime: 8);
                return;

            case SparkEnemyFunction.EmitFallingSparks:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                _fallingSparksBehavior.SpawnFallingSpark(slot);
                SetSparkFunctionTimer(state, additionalTime: 0);
                return;

            default:
                throw new InvalidDataException(
                    $"Spark function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces <c>DEC; BEQ</c>. A zero countdown wraps to <c>$FFFF</c> and therefore does
    /// not expire for another 65,535 actor frames; it must not be treated as immediately due.
    /// </summary>
    private static bool DecrementSparkTimerAndTestExpired(SparkEnemyState state)
    {
        ushort decremented = unchecked((ushort)(state.FunctionTimer - 1));
        if (decremented == 0)
            return true;

        state.FunctionTimer = decremented;
        return false;
    }

    /// <summary>Ports <c>SetSparkFunctionTimer</c> at <c>$A8:E6F6</c>.</summary>
    private void SetSparkFunctionTimer(SparkEnemyState state, ushort additionalTime)
    {
        ushort baseTime = state.BaseFunctionTime;
        if (unchecked((short)baseTime) < 0)
        {
            Func<ushort> nextRandom = _room.Random.Generate;
            baseTime = unchecked((ushort)((nextRandom() & 0x003f) + 4));
        }

        state.FunctionTimer = unchecked((ushort)(baseTime + additionalTime));
    }

    private static void InstallSparkInstructionList(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = 1;
    }

    private SparkEnemyState RequireSparkState(RoomEnemySlot slot) =>
        _sparkStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Spark state.");

    internal void RegisterSparkInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Spark, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Spark, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeSpark(slot);
        });
    }

    internal void RegisterSparkMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Spark, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Spark, (slot, frame, nmiFrameCounter8) =>
        {
            RunSparkMain(slot, RequireSparkState(slot));
        });
    }

    internal static void RegisterSparkOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessSparkInstruction(slot, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_sparkStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
