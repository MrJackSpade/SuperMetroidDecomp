using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The escape animals species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class EscapeAnimalsBehavior : IEnemySpecies
{
    /// <summary>Provides room events and live Samus state used by both escape-animal branches.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Runs Etecoon's slope-ignoring horizontal move and settling ground probe.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds escape-animal AI to room events and terrain collision for the current enemy system.</summary>
    internal EscapeAnimalsBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Bank-$B3 blank frame selected when an escaped animal deletes its visible actor.</summary>
    private const ushort EmptyBankB3Spritemap = 0x804d;

    /// <summary>Whole-pixel horizontal step used by each Dachora movement-list callback.</summary>
    private const ushort EscapeDachoraPixelsPerCallback = 6;
    /// <summary>Lava Y threshold tested by both families' acid-height branch instructions.</summary>
    private const ushort EscapeAnimalLavaBranchY = 0x00ce;
    /// <summary>One unit of 16.16 downward acceleration applied during Etecoon's exit.</summary>
    private const int EscapeEtecoonGravity = 1 << 16;
    /// <summary>Signed 16.16 horizontal speed used when Etecoon exits without a wall collision.</summary>
    private const int EscapeEtecoonUncollidedExitSpeed = 0x00038000;

    /// <summary>Typed Etecoon dispatch state indexed by stable enemy slot, cleared on room load.</summary>
    internal readonly EscapeEtecoonEnemyState?[] _escapeEtecoonStates =
        new EscapeEtecoonEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Typed Dachora state indexed by stable enemy slot, cleared on room load.</summary>
    private readonly EscapeDachoraEnemyState?[] _escapeDachoraStates =
        new EscapeDachoraEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Clears family-local typed views at the ordinary room-load boundary.</summary>
    internal void ResetEscapeAnimalRoomState()
    {
        Array.Clear(_escapeEtecoonStates);
        Array.Clear(_escapeDachoraStates);
    }

    /// <summary>Ports <c>EscapeEtecoon_Init</c> at <c>$B3:E6CB</c>.</summary>
    internal void InitializeEscapeEtecoon(RoomEnemySlot slot)
    {
        var state = new EscapeEtecoonEnemyState(slot);
        _escapeEtecoonStates[slot.SlotIndex] = state;

        // Event $0F means the animals already escaped on an earlier visit. Native code
        // deletes the actor before changing any animation, position, palette, or AI fields.
        if (HasCrittersEscaped())
        {
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        // In the original property vocabulary, $2000 disables ordinary Samus collision and
        // $0400 marks the actor tangible. In this port those bits retain their independently
        // observed scheduler/collision meanings as ProcessInstructions and
        // IgnoreSamusCollision. Raw $8000 remains unnamed because this family never proves
        // its broader engine-wide meaning.
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.Properties = slot.Properties.With(EnemyProperties.SolidToSamus);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.PaletteIndex = 0;

        EscapeEtecoonInitialization initialization =
            EscapeEtecoonDefinitions.Initialization(slot.Parameter1);
        slot.Position = slot.Position.WithWhole(initialization.Position);
        state.PreInstruction = initialization.PreInstruction;
        slot.CurrentInstruction = initialization.InstructionList;
        state.HorizontalSpeed = initialization.HorizontalSpeed;
    }

    /// <summary>Ports <c>EscapeDachora_Init</c> at <c>$B3:EAE5</c>.</summary>
    internal void InitializeEscapeDachora(RoomEnemySlot slot)
    {
        _escapeDachoraStates[slot.SlotIndex] = new EscapeDachoraEnemyState(slot);
        if (HasCrittersEscaped())
        {
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        // Dachora's population already supplies $2400, but the initializer ORs $2000 again.
        // Expressing the idempotent write documents the executable routine and keeps a
        // synthetic/debug population faithful to retail initialization.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = EmptyBankB3Spritemap;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.CurrentInstruction =
            EscapeDachoraInstructionProgramDefinitions.RunningAroundLowTide;
    }

    /// <summary>Ports <c>EscapeEtecoon_Main</c> and its complete indirect dispatch.</summary>
    private void RunEscapeEtecoonMain(
        RoomEnemySlot slot,
        EscapeEtecoonEnemyState state,
        RoomLevelData? level)
    {
        switch (state.PreInstruction)
        {
            case EscapeEtecoonPreInstruction.Cleared:
                return;

            case EscapeEtecoonPreInstruction.EscapeRight:
                AddEscapeAnimalX(slot, EscapeEtecoonUncollidedExitSpeed);
                return;

            case EscapeEtecoonPreInstruction.WaitForEscapeEvent:
                // $B3:E670 changes only the list pointer. It deliberately leaves the
                // existing instruction timer intact, so the new list begins on the same
                // scheduler boundary the SNES would observe.
                if (HasCrittersEscaped())
                    slot.CurrentInstruction =
                        EscapeEtecoonInstructionProgramDefinitions.ExpressGratitudeThenEscape;
                return;

            case EscapeEtecoonPreInstruction.WalkAndFall:
                RunEscapeEtecoonWalking(slot, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Escape Etecoon pre-instruction $B3:{(ushort)state.PreInstruction:X4} " +
                    "is not translated.");
        }
    }

    /// <summary>Ports <c>$B3:E680</c>, including its collision-only event branch.</summary>
    private void RunEscapeEtecoonWalking(
        RoomEnemySlot slot,
        EscapeEtecoonEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Escape Etecoon walking requires room collision data.");

        // The stored speed is signed 8.8; Enemy_MoveRight_IgnoreSlopes receives signed
        // 16.16, hence the exact eight-bit shift. The event test is inside the collision
        // branch in the retail routine and must not turn an animal around in open space.
        int horizontalDisplacement = SnesSixteenSixteen.FromEightEight((short)state.HorizontalSpeed);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            state.HorizontalSpeed = unchecked((ushort)-state.HorizontalSpeed);
            slot.CurrentInstruction = unchecked((short)state.HorizontalSpeed) < 0
                ? EscapeEtecoonInstructionProgramDefinitions.RunningLeftLowTide
                : EscapeEtecoonInstructionProgramDefinitions.RunningRightLowTide;
            if (HasCrittersEscaped())
                slot.CurrentInstruction =
                    EscapeEtecoonInstructionProgramDefinitions.RunningForEscape;
        }

        // Enemy_MoveDown is called even after horizontal collision, and its carry result is
        // ignored. This one-pixel probe lets the common collision helper settle the actor on
        // the room's actual floor without introducing a bespoke ground model.
        _ = _terrain.MoveEnemyVertically(level, slot, EscapeEtecoonGravity);
    }

    /// <summary>
    /// Handles every family-specific command referenced by the retail Etecoon and Dachora
    /// lists. Common frame, timer, goto, and sleep commands remain in the shared interpreter.
    /// </summary>
    private bool TryProcessEscapeAnimalInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape)
        {
            if (!Enum.IsDefined((EscapeEtecoonInstruction)opcode))
                return false;
            EscapeEtecoonEnemyState state = RequireEscapeEtecoonState(slot);
            switch ((EscapeEtecoonInstruction)opcode)
            {
                case EscapeEtecoonInstruction.SetPreInstruction:
                    state.PreInstruction = (EscapeEtecoonPreInstruction)
                        ReadEscapeAnimalOperand(slot, cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    return true;

                case EscapeEtecoonInstruction.ClearPreInstruction:
                    state.PreInstruction = EscapeEtecoonPreInstruction.Cleared;
                    cursor = unchecked((ushort)(cursor + 2));
                    return true;

                case EscapeEtecoonInstruction.GotoIfAcidYLessThanCE:
                    cursor = EscapeAnimalLavaY(samus) >= EscapeAnimalLavaBranchY
                        ? unchecked((ushort)(cursor + 4))
                        : ReadEscapeAnimalOperand(slot, cursor);
                    return true;

                case EscapeEtecoonInstruction.AddXPosition:
                    slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
                        slot.Position.X.Whole + ReadEscapeAnimalOperand(slot, cursor))));
                    cursor = unchecked((ushort)(cursor + 4));
                    return true;

                default:
                    throw new InvalidOperationException($"Undefined {nameof(EscapeEtecoonInstruction)} {opcode:X4}.");
            }
        }
        else if (slot.EnemyDefinitionPointer == EnemyDefinitionId.DachoraEscape)
        {
            if (!Enum.IsDefined((EscapeDachoraInstruction)opcode))
                return false;
            switch ((EscapeDachoraInstruction)opcode)
            {
                case EscapeDachoraInstruction.GotoIfAcidYLessThanCE:
                    cursor = EscapeAnimalLavaY(samus) >= EscapeAnimalLavaBranchY
                        ? unchecked((ushort)(cursor + 4))
                        : ReadEscapeAnimalOperand(slot, cursor);
                    return true;

                case EscapeDachoraInstruction.GotoIfCrittersEscaped:
                    cursor = HasCrittersEscaped()
                        ? ReadEscapeAnimalOperand(slot, cursor)
                        : unchecked((ushort)(cursor + 4));
                    return true;

                case EscapeDachoraInstruction.MoveLeft:
                    slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
                        slot.Position.X.Whole - EscapeDachoraPixelsPerCallback)));
                    cursor = unchecked((ushort)(cursor + 2));
                    return true;

                case EscapeDachoraInstruction.MoveRight:
                    slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
                        slot.Position.X.Whole + EscapeDachoraPixelsPerCallback)));
                    cursor = unchecked((ushort)(cursor + 2));
                    return true;

                default:
                    throw new InvalidOperationException($"Undefined {nameof(EscapeDachoraInstruction)} {opcode:X4}.");
            }
        }

        return false;
    }

    /// <summary>Samples event $0F for Dachora's instruction-list branch and room revisit handling.</summary>
    private bool HasCrittersEscaped() => _room.Services.Events.Has(EventNumber.CrittersEscaped);

    /// <summary>Treats absent Samus/liquid context as the native maximum Y for acid-height branches.</summary>
    private static ushort EscapeAnimalLavaY(SamusState? samus) =>
        samus?.LiquidPhysics.LavaAcidYPosition ?? ushort.MaxValue;

    /// <summary>Reads the word immediately after a family-owned bank-$B3 instruction opcode.</summary>
    private static ushort ReadEscapeAnimalOperand(RoomEnemySlot slot, ushort instructionCursor) =>
        EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
            slot,
            unchecked((ushort)(instructionCursor + 2)));

    /// <summary>Adds a signed 16.16 displacement with native 32-bit wraparound to the actor's X position.</summary>
    private static void AddEscapeAnimalX(RoomEnemySlot slot, int displacement)
    {
        uint fixedPosition = slot.Position.X.RawValue;
        fixedPosition = unchecked(fixedPosition + (uint)displacement);
        slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(unchecked((uint)fixedPosition)));
    }

    /// <summary>Fetches the typed Etecoon dispatch state after initialization, failing for an uninitialized slot.</summary>
    private EscapeEtecoonEnemyState RequireEscapeEtecoonState(RoomEnemySlot slot) =>
        _escapeEtecoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized escape-Etecoon state.");

    /// <summary>Connects bank-$B3 Etecoon and Dachora initialization callbacks to their definition identities.</summary>
    internal void RegisterEscapeAnimalsInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_EtecoonEscape, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeEscapeEtecoon(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_DachoraEscape, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.DachoraEscape, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeEscapeDachora(slot);
        });
    }

    /// <summary>Registers Etecoon's per-frame indirect AI; Dachora movement remains in its instruction list.</summary>
    internal void RegisterEscapeAnimalsMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_EtecoonEscape, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape, (slot, frame, nmiFrameCounter8) =>
        {
            RunEscapeEtecoonMain(
                slot,
                RequireEscapeEtecoonState(slot),
                frame.Level);
        });
    }

    /// <summary>Routes only the escape families' private branch and movement opcodes into their interpreter.</summary>
    internal void RegisterEscapeAnimalsOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessEscapeAnimalInstruction(slot, context.Samus, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetEscapeAnimalRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }


    /// <summary>Registers Dachora's literal RTL main handler while the common instruction pass performs its movement.</summary>
    internal static void RegisterEscapeDachoraMainAi(EnemyAiTable table)
    {
        table.RegisterMain(EnemyAiRoutine.RTL_B3EB1A, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.DachoraEscape, (slot, frame, nmiFrameCounter8) =>
        {
            // $B3:EB1A is a literal RTL. Dachora's complete movement program lives in
            // its ROM instruction lists and therefore runs later in this same frame.
        });
    }
}
