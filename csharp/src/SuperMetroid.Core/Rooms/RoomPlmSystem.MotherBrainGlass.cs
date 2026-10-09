using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Mother Brain's room-authored glass PLM from <c>$84:D1E6-$D331</c>. This remains a real
/// member of the shared forty-slot PLM pool: the head shot callback increments slot 39's
/// room argument, the ordinary PLM handler polls the ROM thresholds, and every shatter
/// opcode publishes actors for the common eighteen-slot bank-$86 projectile owner.
/// </summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>Bytecode pre-instruction installed by the glass PLM to count qualifying projectile hits.</summary>
    private const ushort MotherBrainGlassPreInstruction = MotherBrainGlassPlmProgramDefinitions.HitPreInstruction;
    /// <summary>Bank-$86 definition pointer used for each shard request emitted by the glass program.</summary>
    private const ushort MotherBrainGlassShardDefinition = 0xcefc;
    /// <summary>Shard spawn requests emitted during the current PLM-handler pass.</summary>
    private readonly List<MotherBrainGlassProjectileRequest>
        _motherBrainGlassProjectileRequests = new();
    /// <summary>Optional query for the area-boss bit tested by the glass destruction program.</summary>
    private Func<BossBits, bool>? _motherBrainHasAreaBossBit;
    /// <summary>Optional query for story events tested by the glass destruction program.</summary>
    private Func<EventNumber, bool>? _hasEvent;
    /// <summary>Optional writer invoked when the glass program sets its destroyed event.</summary>
    private Action<EventNumber>? _setEvent;
    /// <summary>Physical PLM pool index of the glass slot, or -1 when no loaded slot is tracked.</summary>
    private int _motherBrainGlassSlotIndex = -1;
    /// <summary>Whether Mother Brain's room setup has installed the glass PLM.</summary>
    private bool _motherBrainGlassWasLoaded;
    /// <summary>Last room-argument value retained when the glass slot is not currently active.</summary>
    private ushort _motherBrainGlassLastRoomArgument;

    /// <summary>Shard actors requested by the most recent PLM handler pass.</summary>
    public IReadOnlyList<MotherBrainGlassProjectileRequest> MotherBrainGlassProjectileRequests =>
        _motherBrainGlassProjectileRequests;

    /// <summary>Runs setup $D5F6 against the already allocated highest room slot.</summary>
    private void SetupMotherBrainGlassSlot(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        RoomPlmPopulationRecord record,
        PlmSlot slot)
    {
        int physicalSlot = Array.IndexOf(_slots, slot);
        if (record.RecordIndex != 0 || physicalSlot != _slots.Length - 1 ||
            record.BlockX != 9 || record.BlockY != 5 || record.RoomArgument != 0x8000)
        {
            throw new InvalidDataException(
                $"Mother Brain glass population diverged: record={record.RecordIndex}, " +
                $"slot={physicalSlot}, block=({record.BlockX},{record.BlockY}), " +
                $"argument=${record.RoomArgument:X4}.");
        }

        slot.InstructionPointer = RoomPlmInstructionLists.MotherBrainGlass;
        slot.RoomArgument = 0;
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        ushort glassLevelWord = unchecked((ushort)((original.LevelWord & 0x0fff) | 0x8000));
        level.SetForegroundEntry(slot.BlockIndex, glassLevelWord);
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
        streamer.SetLevelEntry(slot.BlockIndex, glassLevelWord);

        _motherBrainGlassSlotIndex = physicalSlot;
        _motherBrainGlassRoomWidth = level.WidthInBlocks;
        _motherBrainGlassWasLoaded = true;
        _motherBrainGlassLastRoomArgument = 0;
    }

    /// <summary>
    /// Implements Mother Brain head shot AI's direct <c>INC $1DC7</c>. This is intentionally
    /// separate from the block-hit pre-instruction: a missile can overlap the brain record
    /// before its terrain probe reaches the glass block, and native code counts that hit
    /// immediately in the enemy collision pass.
    /// </summary>
    public void IncrementMotherBrainGlassRoomArgument()
    {
        if (!_motherBrainGlassWasLoaded)
        {
            throw new InvalidOperationException(
                "Mother Brain head damage requires loaded room PLM $D6DE.");
        }

        if (TryGetMotherBrainGlassSlot(out PlmSlot? slot))
            slot!.RoomArgument = unchecked((ushort)(slot.RoomArgument + 1));
        else
            _motherBrainGlassLastRoomArgument = unchecked((ushort)(
                _motherBrainGlassLastRoomArgument + 1));
    }

    /// <summary>Clears projectile requests so the next PLM pass exposes only its own shard spawns.</summary>
    private void BeginMotherBrainGlassFrame() => _motherBrainGlassProjectileRequests.Clear();

    /// <summary>Clears room-specific glass state and pending shard requests during room teardown.</summary>
    private void ResetMotherBrainGlassState()
    {
        _motherBrainGlassProjectileRequests.Clear();
        _motherBrainHasAreaBossBit = null;
        _motherBrainGlassSlotIndex = -1;
        _motherBrainGlassWasLoaded = false;
        _motherBrainGlassLastRoomArgument = 0;
        _motherBrainGlassRoomWidth = 0;
    }

    /// <summary>Counts qualifying missile hits in the glass slot's room argument and clears its projectile-family timer.</summary>
    /// <param name="slot">PLM slot whose header, projectile type, and hit counter are inspected.</param>
    private static void RunMotherBrainGlassPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.MotherBrainGlass || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != MotherBrainGlassPreInstruction)
        {
            // Header `$D5F6` installs exactly `$D4BF`; a different live word indicates
            // corrupted PLM state or an incorrectly reused slot, not another glass route.
            throw new InvalidDataException(
                $"Mother Brain glass has invalid pre-instruction $84:{slot.PreInstruction:X4}.");
        }

        SamusProjectileFamily family = new SamusProjectileTypeWord(slot.LoopTimer).Family;
        if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
            slot.RoomArgument = unchecked((ushort)(slot.RoomArgument + 1));
        slot.LoopTimer = 0;
    }

    /// <summary>Handles the glass program's custom branches, event updates, and shard emission opcodes.</summary>
    /// <param name="bus">Address space used to read instruction operands from the native PLM stream.</param>
    /// <param name="slot">Glass PLM slot whose instruction pointer and room argument are updated.</param>
    /// <param name="instruction">Opcode already fetched by the shared PLM interpreter.</param>
    /// <returns>True when this instruction belongs to the translated glass program.</returns>
    private bool TryExecuteMotherBrainGlassInstruction(
        ISnesAddressSpace bus,
        PlmSlot slot,
        ushort instruction)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.MotherBrainGlass)
            return false;

        ushort cursor = slot.InstructionPointer;
        switch (instruction)
        {
            case RoomPlmInstructionCodes.InstallPreInstruction:
                slot.PreInstruction = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstructionCodes.GotoIfAreaBossBitSet:
                BossBits bossMask = BossBitMasks.FromCartridge(
                    ReadProgramByte(bus, unchecked((ushort)(cursor + 2))),
                    "Mother Brain glass area-boss branch");
                ushort bossTarget = ReadProgramWord(bus, unchecked((ushort)(cursor + 3)));
                slot.InstructionPointer = _motherBrainHasAreaBossBit?.Invoke(bossMask) == true
                    ? bossTarget
                    : unchecked((ushort)(cursor + 5));
                return true;

            case RoomPlmInstructionCodes.GotoIfEventSet:
                ushort eventNumber = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                EventNumber namedEvent = ResolveMotherBrainGlassEvent(eventNumber);
                ushort eventTarget = ReadProgramWord(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = _hasEvent?.Invoke(namedEvent) == true
                    ? eventTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstructionCodes.GotoIfRoomArgumentLess:
                ushort threshold = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                ushort retryTarget = ReadProgramWord(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = slot.RoomArgument < threshold
                    ? retryTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstructionCodes.SpawnFourMotherBrainGlassShards:
                _soundRequests.Add(CreateSoundRequest(RoomPlmSounds.MotherBrainGlassShattering, MaximumQueued: 15));
                byte blockX = checked((byte)(slot.BlockIndex % _motherBrainGlassRoomWidth));
                byte blockY = checked((byte)(slot.BlockIndex / _motherBrainGlassRoomWidth));
                for (int shard = 0; shard < 4; shard++)
                {
                    ushort parameter = ReadProgramWord(
                        bus,
                        unchecked((ushort)(cursor + 2 + shard * 2)));
                    _motherBrainGlassProjectileRequests.Add(new MotherBrainGlassProjectileRequest(
                        MotherBrainGlassShardDefinition,
                        parameter,
                        blockX,
                        blockY));
                }
                slot.InstructionPointer = unchecked((ushort)(cursor + 10));
                return true;

            case RoomPlmInstructionCodes.SetEvent:
                ushort setEventNumber = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                EventNumber eventToSet = ResolveMotherBrainGlassEvent(setEventNumber);
                (_setEvent ?? throw new InvalidOperationException(
                    "Mother Brain glass has no event writer."))(eventToSet);
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// The translated glass program is only valid for the proven event-$02 operand used by
    /// the retail instruction stream. A changed ROM operand is corruption or unported code,
    /// not permission to leak an integer into the named event API.
    /// </summary>
    private static EventNumber ResolveMotherBrainGlassEvent(ushort eventNumber)
    {
        if (eventNumber != (ushort)EventNumber.MotherBrainGlassDestroyed)
        {
            throw new InvalidDataException(
                $"Mother Brain glass referenced event {eventNumber}, not the proven glass-destroyed event.");
        }

        return EventNumber.MotherBrainGlassDestroyed;
    }

    /// <summary>Room width in blocks used to recover the glass PLM's block coordinates from its slot index.</summary>
    private int _motherBrainGlassRoomWidth;

    /// <summary>Finds the active glass PLM at its tracked pool index and verifies its family header.</summary>
    /// <param name="slot">Receives the active glass slot, or null when its index is stale or inactive.</param>
    /// <returns>True only when the tracked slot is active and still belongs to the glass PLM family.</returns>
    private bool TryGetMotherBrainGlassSlot(out PlmSlot? slot)
    {
        if ((uint)_motherBrainGlassSlotIndex < (uint)_slots.Length)
        {
            PlmSlot candidate = _slots[_motherBrainGlassSlotIndex];
            if (candidate.Active && candidate.HeaderPointer == RoomPlmHeaders.MotherBrainGlass)
            {
                slot = candidate;
                return true;
            }
        }
        slot = null;
        return false;
    }

    /// <summary>Retains the glass hit counter and clears its family marker before the deleted PLM slot can be reused.</summary>
    /// <param name="slot">PLM slot that has just been deleted by the shared interpreter.</param>
    private void OnPlmDeleted(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.MotherBrainGlass)
            return;
        _motherBrainGlassLastRoomArgument = slot.RoomArgument;
        _motherBrainGlassSlotIndex = -1;
        // Deleted physical slots are reusable. Clear the family discriminator now so a
        // later ordinary PLM cannot accidentally inherit glass-only instruction dispatch.
        slot.HeaderPointer = 0;
        slot.PreInstruction = 0;
    }

}

/// <summary>One shard spawn emitted by Mother Brain glass bytecode for the shared enemy-projectile pool.</summary>
/// <param name="DefinitionPointer">Bank-$86 projectile definition requested for allocation; glass shards use <c>$CEFC</c>.</param>
/// <param name="Parameter">Native shard placement-table index, restricted by the consumer to 0, 2, or 4.</param>
/// <param name="PlmBlockX">Zero-based room block column of the glass PLM that emitted the shard.</param>
/// <param name="PlmBlockY">Zero-based room block row of the glass PLM that emitted the shard.</param>
public readonly record struct MotherBrainGlassProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    byte PlmBlockX,
    byte PlmBlockY);
