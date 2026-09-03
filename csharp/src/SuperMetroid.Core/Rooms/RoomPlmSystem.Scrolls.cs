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

    private static bool TryStepScrollPlm(
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

    /// <summary>Runs one scroll/extension setup after the shared allocator chose its ID.</summary>
    private static void SetupScrollSlot(RoomLevelData level, PlmSlot slot, ushort header)
    {
        if (header != ScrollPlmHeader)
        {
            (int collisionType, int behavior) = header switch
            {
                RightwardsExtensionHeader => (5, 0xff),
                LeftwardsExtensionHeader => (5, 0x01),
                DownwardsExtensionHeader => (13, 0xff),
                UpwardsExtensionHeader => (13, 0x01),
                _ => throw new InvalidOperationException(
                    "Validated scroll extension escaped its setup table."),
            };
            ushort originalWord = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
            level.SetForegroundEntry(
                slot.BlockIndex,
                unchecked((ushort)((originalWord & 0x0fff) | (collisionType << 12))));
            level.SetBehavior(slot.BlockIndex, unchecked((byte)behavior));

            // Every extension setup ends in DeletePLM. Clearing this preallocated slot is
            // what permits the following ROM record to reuse the same highest native ID.
            ClearSlot(slot);
            return;
        }

        slot.InstructionPointer = 0xaf8a;
        slot.Scroll = new ScrollPlmState();
        ushort triggerWord = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
        level.SetForegroundEntry(
            slot.BlockIndex,
            unchecked((ushort)((triggerWord & 0x0fff) | 0x3000)));
        level.SetBehavior(slot.BlockIndex, 0x46);
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
