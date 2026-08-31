using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    private const ushort ScrollPlmHeader = 0xb703;
    private const ushort RightwardsExtensionHeader = 0xb63b;
    private const ushort LeftwardsExtensionHeader = 0xb63f;
    private const ushort DownwardsExtensionHeader = 0xb643;
    private const ushort UpwardsExtensionHeader = 0xb647;

    /// <summary>Every live cartridge-authored scroll trigger, in native slot order.</summary>
    public IReadOnlyList<ScrollPlmSnapshot> ScrollPlms => _slots
        .Where(slot => slot.Active && slot.Scroll is not null)
        .Select(slot => new ScrollPlmSnapshot(
            slot.BlockIndex,
            slot.RoomArgument,
            slot.Scroll!.Triggered))
        .ToArray();

    /// <summary>
    /// Loads bank-$84's scroll-trigger family from a room's ordinary six-byte PLM population.
    /// </summary>
    /// <remarks>
    /// This is the reusable subset needed by every room that partitions its camera with red
    /// scroll cells. Header <c>$B703</c> installs type-$3/BTS-$46 special air. The four
    /// extension headers immediately rewrite their own blocks to type $5/$D and delete,
    /// exactly like setup routines <c>$84:B33A-$B364</c>; they therefore consume no live
    /// slot after room loading. The trigger's room argument remains a bank-$8F byte-pair
    /// program and is deliberately interpreted only after Samus touches the authored block.
    /// </remarks>
    public int LoadScrollPopulation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort populationPointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        int loaded = 0;
        ushort cursor = populationPointer;
        for (int recordIndex = 0; recordIndex < 256; recordIndex++)
        {
            ushort header = ReadBank8fWord(bus, cursor);
            if (header == 0)
                return loaded;

            byte blockX = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2)));
            byte blockY = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3)));
            ushort roomArgument = ReadBank8fWord(bus, unchecked((ushort)(cursor + 4)));
            cursor = unchecked((ushort)(cursor + 6));

            if (header is not (ScrollPlmHeader or RightwardsExtensionHeader or
                    LeftwardsExtensionHeader or DownwardsExtensionHeader or
                    UpwardsExtensionHeader))
            {
                continue;
            }

            int blockIndex = level.GetBlockIndex(blockX, blockY);
            if (header != ScrollPlmHeader)
            {
                // Extension setup writes both the collision nibble and BTS synchronously.
                // Signed BTS then walks back to the $B703 origin in bank-$94 collision.
                (int collisionType, int behavior) = header switch
                {
                    RightwardsExtensionHeader => (5, 0xff),
                    LeftwardsExtensionHeader => (5, 0x01),
                    DownwardsExtensionHeader => (13, 0xff),
                    UpwardsExtensionHeader => (13, 0x01),
                    _ => throw new InvalidOperationException(
                        "Validated scroll extension escaped its setup table."),
                };
                ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
                level.SetForegroundEntry(
                    blockIndex,
                    (ushort)((originalWord & 0x0fff) | (collisionType << 12)));
                level.SetBehavior(blockIndex, unchecked((byte)behavior));
                continue;
            }

            PlmSlot? slot = AllocateScrollSlot();
            if (slot is null)
                continue; // SpawnRoomPLM silently drops records once all forty slots fill.

            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0; // PLM_Vars begins clear: the trigger is not touched.
            slot.InstructionPointer = 0xaf8a; // Setup skips the debug timer/draw pair.
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            slot.RoomArgument = roomArgument;
            slot.LoopTimer = 0;
            slot.Item = null;
            slot.Scroll = new ScrollPlmState();

            ushort triggerWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
            level.SetForegroundEntry(blockIndex, (ushort)((triggerWord & 0x0fff) | 0x3000));
            level.SetBehavior(blockIndex, 0x46);
            loaded++;
        }

        throw new InvalidDataException(
            $"Room PLM population $8F:{populationPointer:X4} has no zero terminator.");
    }

    /// <summary>
    /// Applies setup <c>$84:B393</c> to the already-resolved origin of special-air BTS $46.
    /// </summary>
    public bool TryNotifyScrollTouch(int blockIndex)
    {
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Scroll is null)
                continue;

            // Native PLM_Vars bit 15 prevents multiple collision probes in one frame from
            // repeatedly advancing the sleeping instruction pointer.
            if (!slot.Scroll.Triggered)
            {
                slot.Scroll.Triggered = true;
                slot.RestoreLevelWord = 0x8000;
                slot.InstructionPointer = 0xaf8c;
                slot.InstructionTimer = 1;
            }
            return true;
        }
        return false;
    }

    private bool TryStepScrollPlm(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomScrollGrid? scrolls,
        PlmSlot slot)
    {
        if (slot.Scroll is null)
            return false;
        if (!slot.Scroll.Triggered)
            return true; // $86B4 sleep leaves this resident trigger dormant.
        if (scrolls is null)
            throw new InvalidOperationException("A triggered scroll PLM requires the active scroll grid.");

        ushort cursor = slot.RoomArgument;
        for (int pairIndex = 0; pairIndex < RoomScrollGrid.StorageByteCount; pairIndex++)
        {
            byte scrollIndex = bus.ReadByte(0x8f0000 | cursor);
            if ((scrollIndex & 0x80) != 0)
            {
                // Instruction $8B55 clears PLM_Vars and restores type-$3 special air, then
                // the list loops to Sleep. BTS $46 remains unchanged and can wake it again.
                slot.Scroll.Triggered = false;
                slot.RestoreLevelWord = 0;
                slot.InstructionPointer = 0xaf8a;
                ushort triggerWord = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
                level.SetForegroundEntry(
                    slot.BlockIndex,
                    (ushort)((triggerWord & 0x0fff) | 0x3000));
                level.SetBehavior(slot.BlockIndex, 0x46);
                return true;
            }

            byte value = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 1)));
            scrolls.SetStorage(scrollIndex, value);
            cursor = unchecked((ushort)(cursor + 2));
        }

        throw new InvalidDataException(
            $"Scroll PLM data $8F:{slot.RoomArgument:X4} has no negative terminator.");
    }

    private PlmSlot? AllocateScrollSlot()
    {
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            if (!_slots[index].Active)
                return _slots[index];
        }
        return null;
    }

    private sealed class ScrollPlmState
    {
        public bool Triggered { get; set; }
    }
}

public readonly record struct ScrollPlmSnapshot(
    int BlockIndex,
    ushort DataPointer,
    bool Triggered);
