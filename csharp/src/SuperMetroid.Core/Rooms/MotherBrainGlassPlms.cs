using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Mother Brain's glass PLM ($D6DE): its setup, shatter projectiles and the room argument it keeps after deletion.</summary>
internal sealed class MotherBrainGlassPlms
{
    private readonly PlmRoomBindings _bindings;
    private readonly PlmSlotTable _slotTable;
    private readonly PlmSoundQueue _sounds;

    internal MotherBrainGlassPlms(PlmRoomBindings bindings, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    private const ushort MotherBrainGlassPreInstruction = MotherBrainGlassPlmProgramDefinitions.HitPreInstruction;
    private const ushort MotherBrainGlassShardDefinition = 0xcefc;
    private readonly List<MotherBrainGlassProjectileRequest>
        _motherBrainGlassProjectileRequests = new();
    private int _motherBrainGlassSlotIndex = -1;
    private bool _motherBrainGlassWasLoaded;
    private ushort _motherBrainGlassLastRoomArgument;

    /// <summary>Shard actors requested by the most recent PLM handler pass.</summary>
    public IReadOnlyList<MotherBrainGlassProjectileRequest> MotherBrainGlassProjectileRequests =>
        _motherBrainGlassProjectileRequests;

    /// <summary>Runs setup $D5F6 against the already allocated highest room slot.</summary>
    internal void SetupMotherBrainGlassSlot(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        RoomPlmPopulationRecord record,
        PlmSlot slot)
    {
        int physicalSlot = Array.IndexOf(_slotTable._slots, slot);
        if (record.RecordIndex != 0 || physicalSlot != _slotTable._slots.Length - 1 ||
            record.Block != new Position16<RoomBlocks>(9, 5) || record.RoomArgument != 0x8000)
        {
            throw new InvalidDataException(
                $"Mother Brain glass population diverged: record={record.RecordIndex}, " +
                $"slot={physicalSlot}, block=({record.Block.X},{record.Block.Y}), " +
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
        _motherBrainGlassRoomWidth = level.Size.X;
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

    internal void BeginMotherBrainGlassFrame() => _motherBrainGlassProjectileRequests.Clear();

    internal void ResetMotherBrainGlassState()
    {
        _motherBrainGlassProjectileRequests.Clear();
        _motherBrainGlassSlotIndex = -1;
        _motherBrainGlassWasLoaded = false;
        _motherBrainGlassLastRoomArgument = 0;
        _motherBrainGlassRoomWidth = 0;
    }

    internal static void RunMotherBrainGlassPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != PlmHeaderId.MotherBrainGlass || slot.PreInstruction == 0)
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

    internal bool TryExecuteMotherBrainGlassInstruction(
        ISnesAddressSpace bus,
        PlmSlot slot,
        RoomPlmInstruction instruction)
    {
        if (slot.HeaderPointer != PlmHeaderId.MotherBrainGlass)
            return false;

        ushort cursor = slot.InstructionPointer;
        if (instruction is not (
            RoomPlmInstruction.InstallPreInstruction or
            RoomPlmInstruction.GotoIfAreaBossBitSet or
            RoomPlmInstruction.GotoIfEventSet or
            RoomPlmInstruction.GotoIfRoomArgumentLess or
            RoomPlmInstruction.SpawnFourMotherBrainGlassShards or
            RoomPlmInstruction.SetEvent))
            return false;

        switch (instruction)
        {
            case RoomPlmInstruction.InstallPreInstruction:
                slot.PreInstruction = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstruction.GotoIfAreaBossBitSet:
                BossBits bossMask = BossBitMasks.FromCartridge(
                    RoomPlmSystem.ReadProgramByte(bus, unchecked((ushort)(cursor + 2))),
                    "Mother Brain glass area-boss branch");
                ushort bossTarget = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 3)));
                slot.InstructionPointer = _bindings.Bosses?.IsDefeated(bossMask) == true
                    ? bossTarget
                    : unchecked((ushort)(cursor + 5));
                return true;

            case RoomPlmInstruction.GotoIfEventSet:
                ushort eventNumber = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                EventNumber namedEvent = ResolveMotherBrainGlassEvent(eventNumber);
                ushort eventTarget = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = _bindings.Events?.Has(namedEvent) == true
                    ? eventTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstruction.GotoIfRoomArgumentLess:
                ushort threshold = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                ushort retryTarget = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = slot.RoomArgument < threshold
                    ? retryTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstruction.SpawnFourMotherBrainGlassShards:
                _sounds._soundRequests.Add(_sounds.CreateSoundRequest(RoomPlmSounds.MotherBrainGlassShattering, MaximumQueued: 15));
                // The native fragment spawn carries both block coordinates as bytes.
                Position16<RoomBlocks> glassBlock = new(checked((byte)(slot.BlockIndex % _motherBrainGlassRoomWidth)),
                    checked((byte)(slot.BlockIndex / _motherBrainGlassRoomWidth)));
                for (int shard = 0; shard < 4; shard++)
                {
                    ushort parameter = RoomPlmSystem.ReadProgramWord(
                        bus,
                        unchecked((ushort)(cursor + 2 + shard * 2)));
                    _motherBrainGlassProjectileRequests.Add(new MotherBrainGlassProjectileRequest(
                        MotherBrainGlassShardDefinition,
                        parameter,
                        glassBlock));
                }
                slot.InstructionPointer = unchecked((ushort)(cursor + 10));
                return true;

            case RoomPlmInstruction.SetEvent:
                ushort setEventNumber = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                EventNumber eventToSet = ResolveMotherBrainGlassEvent(setEventNumber);
                (_bindings.Events ?? throw new InvalidOperationException(
                    "Mother Brain glass has no event writer.")).Set(eventToSet);
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            default:
                throw new InvalidOperationException($"Unhandled RoomPlmInstruction {instruction}.");
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

    private int _motherBrainGlassRoomWidth;

    private bool TryGetMotherBrainGlassSlot(out PlmSlot? slot)
    {
        if ((uint)_motherBrainGlassSlotIndex < (uint)_slotTable._slots.Length)
        {
            PlmSlot candidate = _slotTable._slots[_motherBrainGlassSlotIndex];
            if (candidate.Active && candidate.HeaderPointer == PlmHeaderId.MotherBrainGlass)
            {
                slot = candidate;
                return true;
            }
        }
        slot = null;
        return false;
    }

    internal void OnPlmDeleted(PlmSlot slot)
    {
        if (slot.HeaderPointer != PlmHeaderId.MotherBrainGlass)
            return;
        _motherBrainGlassLastRoomArgument = slot.RoomArgument;
        _motherBrainGlassSlotIndex = -1;
        // Deleted physical slots are reusable. Clear the family discriminator now so a
        // later ordinary PLM cannot accidentally inherit glass-only instruction dispatch.
        slot.HeaderPointer = 0;
        slot.PreInstruction = 0;
    }
}
