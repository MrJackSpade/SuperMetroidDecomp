using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The room's scroll triggers and scroll extensions: their setup, and Samus's touch of a trigger.</summary>
[RoomLifetime]
internal sealed class ScrollObjects
{
    /// <summary>The room's objects, in the update order, searched for the trigger at a block.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds the scroll triggers to the room's objects.</summary>
    internal ScrollObjects(RoomObjectSlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Every live room-authored scroll trigger, in place order.</summary>
    public IReadOnlyList<ScrollObjectSnapshot> Snapshots => _slotTable.FromLastPlace
        .Where(slot => slot.Active && slot.Behaviour is ScrollTriggerObject)
        .Select(slot => new ScrollObjectSnapshot(slot.Block))
        .ToArray();

    /// <summary>
    /// Applies the scroll trigger's touch to the already-resolved origin of its special-air trigger block.
    /// </summary>
    /// <param name="block">The room block of the trigger origin, after any scroll-extension traversal.</param>
    /// <returns>True when a live scroll trigger owns the block, including an already-touched one; false when none does.</returns>
    /// <remarks>
    /// The first touch makes the trigger's writes run on the next room-object update; further collision probes before
    /// then change nothing. The first trigger at the block in the update order takes the touch.
    /// </remarks>
    public bool TryNotifyScrollTouch(Position16<RoomBlocks> block)
    {
        foreach (RoomObjectSlot slot in _slotTable.FromLastPlace)
        {
            if (!slot.Active || slot.Block != block || slot.Behaviour is not ScrollTriggerObject trigger)
                continue;
            trigger.Touch();
            return true;
        }
        return false;
    }

    /// <summary>Runs one scroll trigger's or extension's setup at the place the room population took for it.</summary>
    internal static void SetupScrollSlot(RoomLevelData level, RoomObjectSlot slot,
        RoomObjectHeaderId header, IReadOnlyList<RoomScrollWrite>? program, RoomScrollChange? compiledProgram)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(slot);
        if (header != RoomObjectHeaderId.ScrollTrigger)
        {
            (RoomCollisionType collisionType, int offset) = ScrollExtensionDefinitions.Extensions.TryGetValue(header, out ScrollExtension extension)
                ? extension
                : throw new InvalidOperationException($"Room object {header} is neither a scroll trigger nor a scroll extension.");
            level.SetTerrain(
                slot.Block,
                level.GetCollisionBlock(slot.Block).Terrain.WithCollision(collisionType));
            level.SetBehavior(slot.Block, RoomBlockBehavior.Extending(offset));

            // An extension's whole effect is its setup, which removes it at once: the following authored record takes
            // its place.
            slot.Delete();
            return;
        }

        var trigger = new ScrollTriggerObject(program, compiledProgram);
        slot.Become(trigger);
        trigger.Rest(level, slot.Block);
    }
}
