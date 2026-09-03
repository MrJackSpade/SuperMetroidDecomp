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
    private const ushort MotherBrainGlassInitialInstruction = 0xd202;
    private const ushort MotherBrainGlassPreInstruction = 0xd1e6;
    private const ushort MotherBrainGlassShardDefinition = 0xcefc;
    private const int MotherBrainGlassDestroyedEvent = 2;

    private readonly List<MotherBrainGlassProjectileRequest>
        _motherBrainGlassProjectileRequests = new();
    private Func<byte, bool>? _motherBrainHasAreaBossBit;
    private Func<int, bool>? _motherBrainHasEvent;
    private Action<int>? _motherBrainSetEvent;
    private int _motherBrainGlassSlotIndex = -1;
    private bool _motherBrainGlassWasLoaded;
    private bool _motherBrainGlassWasDeleted;
    private ushort _motherBrainGlassLastRoomArgument;

    /// <summary>Shard actors requested by the most recent PLM handler pass.</summary>
    public IReadOnlyList<MotherBrainGlassProjectileRequest> MotherBrainGlassProjectileRequests =>
        _motherBrainGlassProjectileRequests;

    /// <summary>Whether the cartridge's room population installed PLM <c>$D6DE</c>.</summary>
    public bool MotherBrainGlassWasLoaded => _motherBrainGlassWasLoaded;

    /// <summary>Whether the final event-setting instruction deleted the glass PLM.</summary>
    public bool MotherBrainGlassWasDeleted => _motherBrainGlassWasDeleted;

    /// <summary>Current native room argument, retained after the header deletes itself.</summary>
    public ushort MotherBrainGlassRoomArgument => TryGetMotherBrainGlassSlot(out PlmSlot? slot)
        ? slot!.RoomArgument
        : _motherBrainGlassLastRoomArgument;

    /// <summary>Current bank-$84 instruction pointer, or zero after deletion.</summary>
    public ushort MotherBrainGlassInstructionPointer => TryGetMotherBrainGlassSlot(out PlmSlot? slot)
        ? slot!.InstructionPointer
        : (ushort)0;

    /// <summary>Current PLM instruction countdown, or zero after deletion.</summary>
    public ushort MotherBrainGlassInstructionTimer => TryGetMotherBrainGlassSlot(out PlmSlot? slot)
        ? slot!.InstructionTimer
        : (ushort)0;

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

        slot.InstructionPointer = MotherBrainGlassInitialInstruction;
        slot.RoomArgument = 0;
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        ushort glassLevelWord = unchecked((ushort)((original.LevelWord & 0x0fff) | 0x8000));
        level.SetForegroundEntry(slot.BlockIndex, glassLevelWord);
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
        streamer.SetLevelEntry(slot.BlockIndex, glassLevelWord);

        _motherBrainGlassSlotIndex = physicalSlot;
        _motherBrainGlassRoomWidth = level.WidthInBlocks;
        _motherBrainGlassWasLoaded = true;
        _motherBrainGlassWasDeleted = false;
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

    /// <summary>
    /// Publishes the projectile family to the existing PLM at this block. Setup `$D5F6`
    /// changes the origin to type-$8/BTS-$44`; its pre-instruction accepts only family
    /// <c>$0100</c> missiles and <c>$0200</c> supers, then clears this timer word every frame.
    /// </summary>
    public bool TryNotifyProjectileHit(int blockIndex, ushort projectileType)
    {
        if (!TryGetMotherBrainGlassSlot(out PlmSlot? slot) || slot!.BlockIndex != blockIndex)
            return false;
        slot.LoopTimer = projectileType;
        return true;
    }

    private void BeginMotherBrainGlassFrame() => _motherBrainGlassProjectileRequests.Clear();

    private void ResetMotherBrainGlassState()
    {
        _motherBrainGlassProjectileRequests.Clear();
        _motherBrainHasAreaBossBit = null;
        _motherBrainHasEvent = null;
        _motherBrainSetEvent = null;
        _motherBrainGlassSlotIndex = -1;
        _motherBrainGlassWasLoaded = false;
        _motherBrainGlassWasDeleted = false;
        _motherBrainGlassLastRoomArgument = 0;
        _motherBrainGlassRoomWidth = 0;
    }

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
                slot.PreInstruction = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstructionCodes.GotoIfAreaBossBitSet:
                byte bossMask = bus.ReadByte(Bank84(unchecked((ushort)(cursor + 2))));
                ushort bossTarget = ReadBank84Word(bus, unchecked((ushort)(cursor + 3)));
                slot.InstructionPointer = _motherBrainHasAreaBossBit?.Invoke(bossMask) == true
                    ? bossTarget
                    : unchecked((ushort)(cursor + 5));
                return true;

            case RoomPlmInstructionCodes.GotoIfEventSet:
                ushort eventNumber = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                ushort eventTarget = ReadBank84Word(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = _motherBrainHasEvent?.Invoke(eventNumber) == true
                    ? eventTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstructionCodes.GotoIfRoomArgumentLess:
                ushort threshold = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                ushort retryTarget = ReadBank84Word(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = slot.RoomArgument < threshold
                    ? retryTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstructionCodes.SpawnFourMotherBrainGlassShards:
                _soundRequests.Add(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x2e), MaximumQueued: 15));
                byte blockX = checked((byte)(slot.BlockIndex % _motherBrainGlassRoomWidth));
                byte blockY = checked((byte)(slot.BlockIndex / _motherBrainGlassRoomWidth));
                for (int shard = 0; shard < 4; shard++)
                {
                    ushort parameter = ReadBank84Word(
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
                ushort setEventNumber = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                if (setEventNumber != MotherBrainGlassDestroyedEvent)
                {
                    throw new InvalidDataException(
                        $"Mother Brain glass attempted to set event {setEventNumber}, not event 2.");
                }
                (_motherBrainSetEvent ?? throw new InvalidOperationException(
                    "Mother Brain glass has no event writer."))(setEventNumber);
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            default:
                return false;
        }
    }

    private int _motherBrainGlassRoomWidth;

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

    private void OnPlmDeleted(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.MotherBrainGlass)
            return;
        _motherBrainGlassLastRoomArgument = slot.RoomArgument;
        _motherBrainGlassWasDeleted = true;
        _motherBrainGlassSlotIndex = -1;
        // Deleted physical slots are reusable. Clear the family discriminator now so a
        // later ordinary PLM cannot accidentally inherit glass-only instruction dispatch.
        slot.HeaderPointer = 0;
        slot.PreInstruction = 0;
    }

    private static ushort ReadBank8fWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(
            bus.ReadByte((int)new SnesAddress(0x8f, pointer)) |
            (bus.ReadByte((int)new SnesAddress(0x8f, unchecked((ushort)(pointer + 1)))) << 8)));
}

/// <summary>One <c>SpawnEprojWithRoomGfx($CEFC, parameter)</c> emitted by glass bytecode.</summary>
public readonly record struct MotherBrainGlassProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    byte PlmBlockX,
    byte PlmBlockY);
