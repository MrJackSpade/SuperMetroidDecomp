using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Every live cartridge-authored scroll trigger, in native slot order.</summary>
    public IReadOnlyList<ScrollPlmSnapshot> ScrollPlms => _slots
        .Where(slot => slot.Active && slot.Scroll is not null)
        .Select(slot => new ScrollPlmSnapshot(
            slot.BlockIndex))
        .ToArray();

    /// <summary>
    /// Applies setup <c>$84:B393</c> to the already-resolved origin of special-air BTS $46.
    /// </summary>
    /// <param name="blockIndex">Zero-based row-major 16x16 room-block index of the trigger origin, after any scroll-extension traversal.</param>
    /// <returns>True when a live resident scroll PLM owns the block, including an already-triggered one; false when no matching owner exists.</returns>
    /// <remarks>The first touch latches the trigger and wakes its instruction list with a one-tick timer; repeated collision probes do not wake it again before ordered scroll writes finish.</remarks>
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

    /// <summary>Advances a resident scroll trigger, applying its ordered writes once it has been touched.</summary>
    /// <param name="level">Room collision and foreground data updated when the scroll program completes.</param>
    /// <param name="scrolls">Active scroll grid receiving program writes; required for a triggered slot.</param>
    /// <param name="slot">Resident scroll-trigger PLM whose wait state or program is advanced.</param>
    /// <returns><see langword="true"/> when the slot is a scroll trigger, including while it remains dormant.</returns>
    /// <exception cref="InvalidDataException">The program is truncated, malformed, or lacks its negative terminator.</exception>
    /// <exception cref="InvalidOperationException">A triggered scroll trigger is advanced without its active scroll grid.</exception>
    private static bool TryStepScrollPlm(
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

        // Retail and historical identity-only states execute ordered writes without a decoded cache.
        if (slot.Scroll.Program is null)
        {
            RoomPlmScrollProgramDefinitions.Apply(slot.Scroll.CompiledSource ?? slot.RoomArgument, scrolls.SetStorage);
            FinishScrollMutation(level, slot);
            return true;
        }
        ReadOnlyMemory<byte> program = slot.Scroll.Program;
        for (int pairIndex = 0; pairIndex < RoomScrollGrid.StorageByteCount; pairIndex++)
        {
            int offset = pairIndex * 2;
            if (offset >= program.Length)
                throw new InvalidDataException(
                    $"Scroll PLM program $8F:{slot.RoomArgument:X4} ended without a terminator.");
            byte scrollIndex = program.Span[offset];
            if ((scrollIndex & 0x80) != 0)
            {
                FinishScrollMutation(level, slot);
                return true;
            }

            if (offset + 1 >= program.Length)
                throw new InvalidDataException(
                    $"Scroll PLM program $8F:{slot.RoomArgument:X4} lacks a state byte.");
            byte value = program.Span[offset + 1];
            scrolls.SetStorage(
                scrollIndex,
                RoomScrollStates.FromCartridge(
                    value,
                    $"scroll PLM program $8F:{slot.RoomArgument:X4} pair {pairIndex}"));
        }

        throw new InvalidDataException(
            $"Scroll PLM data $8F:{slot.RoomArgument:X4} has no negative terminator.");
    }

    /// <summary>Restores the trigger's special-air block and waiting instruction after its scroll writes finish.</summary>
    /// <param name="level">Room data whose trigger block is restored.</param>
    /// <param name="slot">Scroll-trigger slot whose latch and instruction pointer are reset.</param>
    private static void FinishScrollMutation(RoomLevelData level, PlmSlot slot)
    {
        // Instruction $8B55 clears PLM_Vars and restores type-$3 special air, then
        // the list loops to Sleep. BTS $46 remains unchanged and can wake it again.
        slot.Scroll!.Triggered = false;
        slot.RestoreLevelWord = 0;
        slot.InstructionPointer = RoomPlmInstructionLists.ScrollTriggerWaiting;
        ushort triggerWord = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
        level.SetForegroundEntry(
            slot.BlockIndex,
            (ushort)((triggerWord & 0x0fff) | 0x3000));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ScrollTrigger);
    }

    /// <summary>Runs one scroll/extension setup after the shared allocator chose its ID.</summary>
    private static void SetupScrollSlot(RoomLevelData level, PlmSlot slot,
        ushort header, ReadOnlySpan<byte> program, ushort? compiledSource)
    {
        if (header != RoomPlmHeaders.ScrollTrigger)
        {
            (int collisionType, int behavior) = header switch
            {
                RoomPlmHeaders.RightwardsScrollExtension => (5, 0xff),
                RoomPlmHeaders.LeftwardsScrollExtension => (5, 0x01),
                RoomPlmHeaders.DownwardsScrollExtension => (13, 0xff),
                RoomPlmHeaders.UpwardsScrollExtension => (13, 0x01),
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

        slot.InstructionPointer = RoomPlmInstructionLists.ScrollTriggerWaiting;
        slot.Scroll = new ScrollPlmState
        {
            UseCompiledRetailProgram = true,
            Program = compiledSource.HasValue ? null : program.ToArray(),
            CompiledSource = compiledSource,
        };
        ushort triggerWord = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
        level.SetForegroundEntry(
            slot.BlockIndex,
            unchecked((ushort)((triggerWord & 0x0fff) | 0x3000)));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ScrollTrigger);
    }

    /// <summary>Runtime latch and source data for a resident scroll-trigger PLM.</summary>
    private sealed class ScrollPlmState
    {
        /// <summary>Whether contact has awakened this trigger's instruction list to apply its scroll writes.</summary>
        public bool Triggered { get; set; }
        /// <summary>Bound retail program identity; independent of later room-argument edits.</summary>
        public ushort? CompiledSource { get; set; }
        /// <summary>Legacy snapshot field only; it no longer selects an input reader.</summary>
        public bool UseCompiledRetailProgram { get; set; }
        /// <summary>Validated constructed pairs. Null executes the retail identity directly, including historical states.</summary>
        public byte[]? Program { get; set; }
    }
}

/// <summary>Detached diagnostic identity of one live resident scroll trigger; it does not expose mutable slot or scroll-program state.</summary>
/// <param name="BlockIndex">Zero-based row-major 16x16 room-block index, not the native doubled block-byte offset.</param>
public readonly record struct ScrollPlmSnapshot(
    int BlockIndex);
