using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Scroll PLMs: setup, stepping and the scroll-cell mutation they apply when Samus touches them.</summary>
internal sealed class ScrollObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the Scroll family's step.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.Scroll, static (slot, context) =>
            TryStepScrollObject(context.Level, context.Frame.Scrolls, slot) ? RoomObjectStepOutcome.Consumed : RoomObjectStepOutcome.Shared);
    }

    /// <summary>PLM slots, in the game's search order, searched for resident scroll triggers.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds scroll-trigger operations to the room's PLM slot table.</summary>
    internal ScrollObjects(RoomObjectSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>Every live room-authored scroll trigger, by slot ordinal.</summary>
    public IReadOnlyList<ScrollObjectSnapshot> Snapshots => _slotTable.Slots
        .Where(slot => slot.Active && slot.Family is ScrollObjectState)
        .Select(slot => new ScrollObjectSnapshot(
            slot.BlockIndex))
        .ToArray();

    /// <summary>
    /// Applies the scroll trigger's touch to the already-resolved origin of its special-air trigger block.
    /// </summary>
    /// <param name="blockIndex">Zero-based row-major 16x16 room-block index of the trigger origin, after any scroll-extension traversal.</param>
    /// <returns>True when a live resident scroll PLM owns the block, including an already-triggered one; false when no matching owner exists.</returns>
    /// <remarks>The first touch latches the trigger and wakes its instruction list with a one-tick timer; repeated collision probes do not wake it again before ordered scroll writes finish.</remarks>
    public bool TryNotifyScrollTouch(int blockIndex)
    {
        foreach (RoomObjectSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Family is not ScrollObjectState scroll)
                continue;

            // The trigger latch prevents multiple collision probes in one frame from repeatedly advancing the sleeping
            // program.
            if (!scroll.Triggered)
            {
                scroll.Triggered = true;
                slot.FrameHold = 1;
            }
            return true;
        }
        return false;
    }

    /// <summary>Runs ordered scroll writes after a trigger wakes, or preserves its dormant sleep.</summary>
    internal static bool TryStepScrollObject(
        RoomLevelData level,
        RoomScrollGrid? scrolls,
        RoomObjectSlot slot)
    {
        if (slot.Family is not ScrollObjectState scroll)
            return false;
        if (!scroll.Triggered)
            return true; // The sleep leaves this resident trigger dormant.
        if (scrolls is null)
            throw new InvalidOperationException("A triggered scroll PLM requires the active scroll grid.");

        // A constructed trigger runs its validated writes, a retail one its program's ordered writes; either program
        // ends after its last write.
        if (scroll.Program is null)
        {
            RoomObjectScrollPrograms.Apply(scroll.CompiledProgram ?? throw new InvalidOperationException(
                "A scroll trigger has neither a retail program nor constructed writes."), scrolls.SetCell);
        }
        else
        {
            foreach (RoomScrollWrite write in scroll.Program)
                scrolls.SetCell(write.Index, write.State);
        }
        FinishScrollMutation(level, slot);
        return true;
    }

    /// <summary>Restores the special-air trigger and rearms its waiting program.</summary>
    private static void FinishScrollMutation(RoomLevelData level, RoomObjectSlot slot)
    {
        // The finishing instruction clears the trigger latch and restores special air, then the program loops to
        // sleep. The scroll-trigger behaviour remains unchanged and can wake it again.
        ((slot.Family as ScrollObjectState) ?? throw new InvalidOperationException("A scroll PLM slot owns scroll trigger state.")).Triggered = false;
        level.SetTerrain(
            slot.BlockIndex,
            level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(RoomCollisionType.SpecialAir));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Reacting(BlockReactionDefinitions.ScrollTrigger));
    }

    /// <summary>Runs one scroll/extension setup after the shared allocator chose its ID.</summary>
    internal static void SetupScrollSlot(RoomLevelData level, RoomObjectSlot slot,
        RoomObjectHeaderId header, IReadOnlyList<RoomScrollWrite>? program, RoomScrollProgram? compiledProgram)
    {
        if (header != RoomObjectHeaderId.ScrollTrigger)
        {
            (RoomCollisionType collisionType, int offset) = ScrollExtensionDefinitions.Extensions.TryGetValue(header, out ScrollExtension extension)
                ? extension
                : throw new InvalidOperationException($"Room object {header} is neither a scroll trigger nor a scroll extension.");
            level.SetTerrain(
                slot.BlockIndex,
                level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(collisionType));
            level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Extending(offset));

            // Every extension setup ends in DeletePLM. Clearing this preallocated slot is
            // what permits the following authored record to reuse the same highest ID.
            slot.Clear();
            return;
        }

        slot.Family = new ScrollObjectState
        {
            Program = compiledProgram.HasValue ? null : program?.ToArray() ?? throw new InvalidOperationException(
                "A scroll trigger has neither a retail program nor constructed writes."),
            CompiledProgram = compiledProgram,
        };
        level.SetTerrain(
            slot.BlockIndex,
            level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(RoomCollisionType.SpecialAir));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Reacting(BlockReactionDefinitions.ScrollTrigger));
    }

    /// <summary>Per-PLM scroll trigger latch and its bound retail or constructed write program.</summary>
    internal sealed class ScrollObjectState : RoomObjectFamilyState
    {
        /// <inheritdoc/>
        internal override RoomObjectFamily Family => RoomObjectFamily.Scroll;

        /// <summary>Latched on first touch until ordered scroll writes finish.</summary>
        public bool Triggered { get; set; }
        /// <summary>The retail program the trigger runs; independent of later room-argument edits.</summary>
        public RoomScrollProgram? CompiledProgram { get; set; }
        /// <summary>Validated constructed writes, in order. Null runs the retail program.</summary>
        public RoomScrollWrite[]? Program { get; set; }
    }
}
