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
    private const ushort MotherBrainGlassHeader = 0xd6de;
    private const ushort MotherBrainGlassInitialInstruction = 0xd202;
    private const ushort MotherBrainGlassPreInstruction = 0xd1e6;
    private const ushort InstallPreInstruction = 0x86c1;
    private const ushort GotoIfAreaBossBitSet = 0x880e;
    private const ushort GotoIfEventSet = 0x882d;
    private const ushort SetEvent = 0x883e;
    private const ushort GotoIfRoomArgumentLess = 0xd2f9;
    private const ushort SpawnFourMotherBrainGlassShards = 0xd30b;
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

    /// <summary>
    /// Scans one bank-$8F room population for the sole translated loaded PLM family. The
    /// Mother Brain room contains exactly one record, <c>$D6DE</c> at block (9,5), followed
    /// by the zero header terminator. Its setup clears the record argument and changes only
    /// the collision nibble/BTS; the existing twelve-bit visual block remains untouched.
    /// </summary>
    public bool TryLoadMotherBrainGlassPopulation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        ushort populationPointer,
        Func<byte, bool> hasAreaBossBit,
        Func<int, bool> hasEvent,
        Action<int> setEvent)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);
        ArgumentNullException.ThrowIfNull(hasAreaBossBit);
        ArgumentNullException.ThrowIfNull(hasEvent);
        ArgumentNullException.ThrowIfNull(setEvent);

        ushort cursor = populationPointer;
        for (int recordIndex = 0; recordIndex < 256; recordIndex++)
        {
            ushort header = ReadBank8fWord(bus, cursor);
            if (header == 0)
                return false;

            byte blockX = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2)));
            byte blockY = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3)));
            ushort roomArgument = ReadBank8fWord(bus, unchecked((ushort)(cursor + 4)));
            cursor = unchecked((ushort)(cursor + 6));
            if (header != MotherBrainGlassHeader)
                continue;

            // The head callback writes PLM room-argument slot $4E directly. That hardcoded
            // address is valid only because this is the first room record and therefore
            // receives the highest free physical slot. Reject altered ordering instead of
            // quietly attaching the callback to a different host object.
            if (recordIndex != 0 || blockX != 9 || blockY != 5 || roomArgument != 0x8000 ||
                ReadBank8fWord(bus, cursor) != 0)
            {
                throw new InvalidDataException(
                    $"Mother Brain glass population diverged: record={recordIndex}, " +
                    $"block=({blockX},{blockY}), argument=${roomArgument:X4}.");
            }

            PlmSlot slot = _slots[^1];
            if (slot.Active)
                return false; // Native descending allocator silently drops a full-pool spawn.

            slot.Active = true;
            slot.HeaderPointer = MotherBrainGlassHeader;
            slot.BlockIndex = level.GetBlockIndex(blockX, blockY);
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = MotherBrainGlassInitialInstruction;
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            slot.RoomArgument = 0; // Setup `$84:D5F6` overwrites population argument $8000.
            slot.LoopTimer = 0;

            RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
            ushort glassLevelWord = unchecked((ushort)((original.LevelWord & 0x0fff) | 0x8000));
            level.SetForegroundEntry(slot.BlockIndex, glassLevelWord);
            level.SetBehavior(slot.BlockIndex, 0x44);
            streamer.SetLevelEntry(slot.BlockIndex, glassLevelWord);

            _motherBrainGlassSlotIndex = _slots.Length - 1;
            _motherBrainGlassRoomWidth = level.WidthInBlocks;
            _motherBrainGlassWasLoaded = true;
            _motherBrainGlassWasDeleted = false;
            _motherBrainGlassLastRoomArgument = 0;
            _motherBrainHasAreaBossBit = hasAreaBossBit;
            _motherBrainHasEvent = hasEvent;
            _motherBrainSetEvent = setEvent;
            return true;
        }

        throw new InvalidDataException(
            $"Room PLM population $8F:{populationPointer:X4} has no zero terminator.");
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

    private void RunMotherBrainGlassPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != MotherBrainGlassHeader || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != MotherBrainGlassPreInstruction)
        {
            throw new NotSupportedException(
                $"Mother Brain glass pre-instruction $84:{slot.PreInstruction:X4} is not translated.");
        }

        ushort family = unchecked((ushort)(slot.LoopTimer & 0x0f00));
        if (family is 0x0100 or 0x0200)
            slot.RoomArgument = unchecked((ushort)(slot.RoomArgument + 1));
        slot.LoopTimer = 0;
    }

    private bool TryExecuteMotherBrainGlassInstruction(
        ISnesAddressSpace bus,
        PlmSlot slot,
        ushort instruction)
    {
        if (slot.HeaderPointer != MotherBrainGlassHeader)
            return false;

        ushort cursor = slot.InstructionPointer;
        switch (instruction)
        {
            case InstallPreInstruction:
                slot.PreInstruction = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case GotoIfAreaBossBitSet:
                byte bossMask = bus.ReadByte(0x840000 | unchecked((ushort)(cursor + 2)));
                ushort bossTarget = ReadBank84Word(bus, unchecked((ushort)(cursor + 3)));
                slot.InstructionPointer = _motherBrainHasAreaBossBit?.Invoke(bossMask) == true
                    ? bossTarget
                    : unchecked((ushort)(cursor + 5));
                return true;

            case GotoIfEventSet:
                ushort eventNumber = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                ushort eventTarget = ReadBank84Word(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = _motherBrainHasEvent?.Invoke(eventNumber) == true
                    ? eventTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case GotoIfRoomArgumentLess:
                ushort threshold = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                ushort retryTarget = ReadBank84Word(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = slot.RoomArgument < threshold
                    ? retryTarget
                    : unchecked((ushort)(cursor + 6));
                return true;

            case SpawnFourMotherBrainGlassShards:
                _soundRequests.Add(new PlmSoundRequest(Library: 3, SoundId: 0x2e, MaximumQueued: 15));
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

            case SetEvent:
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
            if (candidate.Active && candidate.HeaderPointer == MotherBrainGlassHeader)
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
        if (slot.HeaderPointer != MotherBrainGlassHeader)
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
            bus.ReadByte(0x8f0000 | pointer) |
            (bus.ReadByte(0x8f0000 | unchecked((ushort)(pointer + 1))) << 8)));
}

/// <summary>One <c>SpawnEprojWithRoomGfx($CEFC, parameter)</c> emitted by glass bytecode.</summary>
public readonly record struct MotherBrainGlassProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    byte PlmBlockX,
    byte PlmBlockY);
