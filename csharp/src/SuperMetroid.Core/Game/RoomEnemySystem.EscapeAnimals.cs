using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$B3 pre-instruction words stored by escape Etecoon enemy <c>$F2D3</c>. These are
/// executable cartridge addresses, not host state identifiers: the main AI at
/// <c>$B3:E655</c> performs an indirect call through common enemy variable F every frame.
/// </summary>
public enum EscapeEtecoonPreInstruction : ushort
{
    /// <summary><c>$B3:807B</c>, the bank-common one-byte RTL installed by opcode $8074.</summary>
    Cleared = 0x807b,

    /// <summary><c>$B3:E65C</c>, add 3.5 pixels to X without terrain collision.</summary>
    EscapeRight = 0xe65c,

    /// <summary><c>$B3:E670</c>, wait for the persistent “critters escaped” event.</summary>
    WaitForEscapeEvent = 0xe670,

    /// <summary><c>$B3:E680</c>, walk, reverse at walls, and remain floor-aligned.</summary>
    WalkAndFall = 0xe680,
}

/// <summary>
/// Typed projection of the only two common words used by escape Etecoon. Keeping the
/// values on <see cref="RoomEnemySlot"/> preserves the original WRAM layout while giving
/// debugger users the meanings established by <c>$B3:E655-$E730</c>.
/// </summary>
public sealed class EscapeEtecoonEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal EscapeEtecoonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A, a signed 8.8 horizontal speed consumed by <c>$B3:E680</c>.</summary>
    public ushort HorizontalSpeed
    {
        get => _slot.VariableA;
        set => _slot.VariableA = value;
    }

    /// <summary>Variable F, the literal bank-$B3 pre-instruction pointer.</summary>
    public EscapeEtecoonPreInstruction PreInstruction
    {
        get => (EscapeEtecoonPreInstruction)_slot.VariableF;
        set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// Debugger-visible typed marker for escape Dachora enemy <c>$F313</c>. Dachora owns no
/// private variables: all of its movement is authored directly in the three ROM instruction
/// lists at <c>$B3:E964-$EAA7</c>. These read-only properties expose that live script state
/// without inventing a parallel host state machine.
/// </summary>
public sealed class EscapeDachoraEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal EscapeDachoraEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ushort InstructionPointer => _slot.CurrentInstruction;
    public ushort InstructionTimer => _slot.InstructionTimer;
    public ushort LoopTimer => _slot.Timer;
}

/// <summary>
/// Literal translation of the four escape-sequence animal records in population
/// <c>$A1:8ED3</c>. Neither family attacks or receives damage: both headers deliberately
/// point touch, shot, hurt, and power-bomb callbacks at bank-local RTL routines. Their real
/// behavior is load-time event gating, ROM animation, collision-aware Etecoon motion, and
/// Dachora's callback-driven sprint.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort EscapeEtecoonDefinition = 0xf2d3;
    internal const ushort EscapeDachoraDefinition = 0xf313;

    private const int EscapeAnimalBank = 0xb30000;
    private const int CrittersEscapedEvent = (int)EventNumber.CrittersEscaped;

    private const ushort EmptyBankB3Spritemap = 0x804d;
    private const ushort EscapeEtecoonLeftWalkList = 0xe556;
    private const ushort EscapeEtecoonRightWalkList = 0xe582;
    private const ushort EscapeEtecoonRightEscapeList = 0xe5ae;
    private const ushort EscapeEtecoonDelayedEscapeList = 0xe5da;
    private const ushort EscapeDachoraNormalList = 0xe964;

    private const ushort EscapeEtecoonXPositionTable = 0xe718;
    private const ushort EscapeEtecoonYPositionTable = 0xe71e;
    private const ushort EscapeEtecoonPreInstructionTable = 0xe724;
    private const ushort EscapeEtecoonInstructionTable = 0xe72a;
    private const ushort EscapeEtecoonSpeedTable = 0xe730;

    private const ushort EscapeEtecoonLavaBranchInstruction = 0xe545;
    private const ushort EscapeEtecoonAddXInstruction = 0xe610;
    private const ushort EscapeDachoraLavaBranchInstruction = 0xeaa8;
    private const ushort EscapeDachoraEventBranchInstruction = 0xeab8;
    private const ushort EscapeDachoraMoveLeftInstruction = 0xeac9;
    private const ushort EscapeDachoraMoveRightInstruction = 0xead7;

    private const ushort EscapeDachoraPixelsPerCallback = 6;
    private const ushort EscapeAnimalLavaBranchY = 0x00ce;
    private const int EscapeEtecoonGravity = 1 << 16;
    private const int EscapeEtecoonUncollidedExitSpeed = 0x00038000;

    private readonly EscapeEtecoonEnemyState?[] _escapeEtecoonStates =
        new EscapeEtecoonEnemyState?[MaximumEnemyCount];
    private readonly EscapeDachoraEnemyState?[] _escapeDachoraStates =
        new EscapeDachoraEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for each physical escape-Etecoon slot.</summary>
    public IReadOnlyList<EscapeEtecoonEnemyState?> EscapeEtecoonStates =>
        _escapeEtecoonStates;

    /// <summary>Typed script state for each physical escape-Dachora slot.</summary>
    public IReadOnlyList<EscapeDachoraEnemyState?> EscapeDachoraStates =>
        _escapeDachoraStates;

    /// <summary>Clears family-local typed views at the ordinary room-load boundary.</summary>
    private void ResetEscapeAnimalRoomState()
    {
        Array.Clear(_escapeEtecoonStates);
        Array.Clear(_escapeDachoraStates);
    }

    /// <summary>Ports <c>EscapeEtecoon_Init</c> at <c>$B3:E6CB</c>.</summary>
    private void InitializeEscapeEtecoon(RoomEnemySlot slot)
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
        slot.Properties = unchecked((ushort)(slot.Properties | 0x8000));
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.PaletteIndex = 0;

        // Parameter 1 is an even byte offset in every retail record (0, 2, 4). The native
        // C expression shifts right to a word index and then indexes ushort tables, which is
        // exactly equivalent to clearing a possible low bit before adding the byte offset.
        // Retaining ROM reads also preserves the cartridge's unchecked behavior for a
        // debugger-edited parameter instead of imposing a host-only three-value restriction.
        ushort tableOffset = unchecked((ushort)(slot.Parameter1 & 0xfffe));
        slot.XPosition = ReadEscapeAnimalWord(EscapeEtecoonXPositionTable, tableOffset);
        slot.YPosition = ReadEscapeAnimalWord(EscapeEtecoonYPositionTable, tableOffset);
        state.PreInstruction = (EscapeEtecoonPreInstruction)ReadEscapeAnimalWord(
            EscapeEtecoonPreInstructionTable,
            tableOffset);
        slot.CurrentInstruction = ReadEscapeAnimalWord(
            EscapeEtecoonInstructionTable,
            tableOffset);
        state.HorizontalSpeed = ReadEscapeAnimalWord(EscapeEtecoonSpeedTable, tableOffset);
    }

    /// <summary>Ports <c>EscapeDachora_Init</c> at <c>$B3:EAE5</c>.</summary>
    private void InitializeEscapeDachora(RoomEnemySlot slot)
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
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = EscapeDachoraNormalList;
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
                    slot.CurrentInstruction = EscapeEtecoonDelayedEscapeList;
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
        int horizontalDisplacement = unchecked((short)state.HorizontalSpeed) << 8;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            slot.InstructionTimer = 1;
            state.HorizontalSpeed = unchecked((ushort)-state.HorizontalSpeed);
            slot.CurrentInstruction = unchecked((short)state.HorizontalSpeed) < 0
                ? EscapeEtecoonLeftWalkList
                : EscapeEtecoonRightWalkList;
            if (HasCrittersEscaped())
                slot.CurrentInstruction = EscapeEtecoonRightEscapeList;
        }

        // Enemy_MoveDown is called even after horizontal collision, and its carry result is
        // ignored. This one-pixel probe lets the common collision helper settle the actor on
        // the room's actual floor without introducing a bespoke ground model.
        _ = MoveEnemyVertically(level, slot, EscapeEtecoonGravity);
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
        if (slot.EnemyDefinitionPointer == EscapeEtecoonDefinition)
        {
            EscapeEtecoonEnemyState state = RequireEscapeEtecoonState(slot);
            switch (opcode)
            {
                case EscapeAnimalInstructionCodes.Instruction_CommonB3_Enemy0FB2_InY:
                    state.PreInstruction = (EscapeEtecoonPreInstruction)ReadEscapeAnimalOperand(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    return true;

                case EscapeAnimalInstructionCodes.Instruction_CommonB3_SetEnemy0FB2ToRTS:
                    state.PreInstruction = EscapeEtecoonPreInstruction.Cleared;
                    cursor = unchecked((ushort)(cursor + 2));
                    return true;

                case EscapeEtecoonLavaBranchInstruction:
                    cursor = EscapeAnimalLavaY(samus) >= EscapeAnimalLavaBranchY
                        ? unchecked((ushort)(cursor + 4))
                        : ReadEscapeAnimalOperand(cursor);
                    return true;

                case EscapeEtecoonAddXInstruction:
                    slot.XPosition = unchecked((ushort)(
                        slot.XPosition + ReadEscapeAnimalOperand(cursor)));
                    cursor = unchecked((ushort)(cursor + 4));
                    return true;
            }
        }
        else if (slot.EnemyDefinitionPointer == EscapeDachoraDefinition)
        {
            switch (opcode)
            {
                case EscapeDachoraLavaBranchInstruction:
                    cursor = EscapeAnimalLavaY(samus) >= EscapeAnimalLavaBranchY
                        ? unchecked((ushort)(cursor + 4))
                        : ReadEscapeAnimalOperand(cursor);
                    return true;

                case EscapeDachoraEventBranchInstruction:
                    cursor = HasCrittersEscaped()
                        ? ReadEscapeAnimalOperand(cursor)
                        : unchecked((ushort)(cursor + 4));
                    return true;

                case EscapeDachoraMoveLeftInstruction:
                    slot.XPosition = unchecked((ushort)(
                        slot.XPosition - EscapeDachoraPixelsPerCallback));
                    cursor = unchecked((ushort)(cursor + 2));
                    return true;

                case EscapeDachoraMoveRightInstruction:
                    slot.XPosition = unchecked((ushort)(
                        slot.XPosition + EscapeDachoraPixelsPerCallback));
                    cursor = unchecked((ushort)(cursor + 2));
                    return true;
            }
        }

        return false;
    }

    private bool HasCrittersEscaped() => RequireEvent(CrittersEscapedEvent);

    private static ushort EscapeAnimalLavaY(SamusState? samus) =>
        samus?.LiquidPhysics.LavaAcidYPosition ?? ushort.MaxValue;

    private ushort ReadEscapeAnimalOperand(ushort instructionCursor) =>
        ReadEscapeAnimalWord(unchecked((ushort)(instructionCursor + 2)), 0);

    private ushort ReadEscapeAnimalWord(ushort basePointer, ushort byteOffset) =>
        ReadWord(_bus!, EscapeAnimalBank | unchecked((ushort)(basePointer + byteOffset)));

    private static void AddEscapeAnimalX(RoomEnemySlot slot, int displacement)
    {
        uint fixedPosition = ((uint)slot.XPosition << 16) | slot.XSubposition;
        fixedPosition = unchecked(fixedPosition + (uint)displacement);
        slot.XPosition = unchecked((ushort)(fixedPosition >> 16));
        slot.XSubposition = unchecked((ushort)fixedPosition);
    }

    private EscapeEtecoonEnemyState RequireEscapeEtecoonState(RoomEnemySlot slot) =>
        _escapeEtecoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized escape-Etecoon state.");
}
