using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Scroll PLMs: setup, stepping and the scroll-cell mutation they apply when Samus touches them.</summary>
internal sealed class ScrollPlms
{
    /// <summary>Native-order PLM slots searched for resident scroll triggers.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds scroll-trigger operations to the room's PLM slot table.</summary>
    internal ScrollPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>Every live cartridge-authored scroll trigger, in native slot order.</summary>
    public IReadOnlyList<ScrollPlmSnapshot> Snapshots => _slotTable._slots
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
        foreach (PlmSlot slot in _slotTable._slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Scroll is null)
                continue;

            // Native PLM_Vars bit 15 prevents multiple collision probes in one frame from
            // repeatedly advancing the sleeping instruction pointer.
            if (!slot.Scroll.Triggered)
            {
                slot.Scroll.Triggered = true;
                slot.RestoreLevelWord = new RoomLevelWord(0).WithCollisionType(RoomCollisionType.SolidBlock).Raw;
                slot.InstructionPointer = RoomPlmInstructionLists.ScrollTriggerTriggered;
                slot.InstructionTimer = NativeCountdown.Start(1);
            }
            return true;
        }
        return false;
    }

    /// <summary>Runs ordered scroll writes after a trigger wakes, or preserves its dormant sleep.</summary>
    internal static bool TryStepScrollPlm(
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
            if (unchecked((sbyte)scrollIndex) < 0)
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

    /// <summary>Restores the type-$3 special-air trigger and rearms its waiting instruction list.</summary>
    private static void FinishScrollMutation(RoomLevelData level, PlmSlot slot)
    {
        // Instruction $8B55 clears PLM_Vars and restores type-$3 special air, then
        // the list loops to Sleep. BTS $46 remains unchanged and can wake it again.
        (slot.Scroll ?? throw new InvalidOperationException("A scroll PLM slot owns scroll trigger state.")).Triggered = false;
        slot.RestoreLevelWord = 0;
        slot.InstructionPointer = RoomPlmInstructionLists.ScrollTriggerWaiting;
        ushort triggerWord = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
        level.SetForegroundEntry(
            slot.BlockIndex,
            new RoomLevelWord(triggerWord).WithCollisionType(RoomCollisionType.SpecialAir).Raw);
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ScrollTrigger);
    }

    /// <summary>Runs one scroll/extension setup after the shared allocator chose its ID.</summary>
    internal static void SetupScrollSlot(RoomLevelData level, PlmSlot slot,
        PlmHeaderId header, ReadOnlySpan<byte> program, ushort? compiledSource)
    {
        if (header != PlmHeaderId.ScrollTrigger)
        {
            (RoomCollisionType collisionType, sbyte offset) = header switch
            {
                PlmHeaderId.RightwardsScrollExtension => (RoomCollisionType.HorizontalExtension, (sbyte)-1),
                PlmHeaderId.LeftwardsScrollExtension => (RoomCollisionType.HorizontalExtension, (sbyte)1),
                PlmHeaderId.DownwardsScrollExtension => (RoomCollisionType.VerticalExtension, (sbyte)-1),
                PlmHeaderId.UpwardsScrollExtension => (RoomCollisionType.VerticalExtension, (sbyte)1),
                _ => throw new InvalidOperationException(
                    "Validated scroll extension escaped its setup table."),
            };
            ushort originalWord = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
            level.SetForegroundEntry(
                slot.BlockIndex,
                new RoomLevelWord(originalWord).WithCollisionType(collisionType).Raw);
            level.SetBehavior(slot.BlockIndex, unchecked((byte)offset));

            // Every extension setup ends in DeletePLM. Clearing this preallocated slot is
            // what permits the following ROM record to reuse the same highest native ID.
            PlmPopulationLoader.ClearSlot(slot);
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
            new RoomLevelWord(triggerWord).WithCollisionType(RoomCollisionType.SpecialAir).Raw);
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ScrollTrigger);
    }

    /// <summary>Per-PLM scroll trigger latch and its bound retail or constructed write program.</summary>
    internal sealed class ScrollPlmState
    {
        /// <summary>Latched on first touch until ordered scroll writes finish.</summary>
        public bool Triggered { get; set; }
        /// <summary>Bound retail program identity; independent of later room-argument edits.</summary>
        public ushort? CompiledSource { get; set; }
        /// <summary>Legacy snapshot field only; it no longer selects an input reader.</summary>
        public bool UseCompiledRetailProgram { get; set; }
        /// <summary>Validated constructed pairs. Null executes the retail identity directly, including historical states.</summary>
        public byte[]? Program { get; set; }
    }
}
