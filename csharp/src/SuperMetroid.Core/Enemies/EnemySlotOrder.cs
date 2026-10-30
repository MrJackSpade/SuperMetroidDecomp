namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How a room orders its enemies (#1471): how many it can hold at once, which empty place a newly spawned enemy takes,
/// and the order enemies update, draw and are reached by a Power Bomb's blast. This is how the game works rather than
/// a hardware leftover: every part of a multipart enemy takes a place of its own, a spawn into a full room fails, a
/// spawn fills the earliest emptied place, and enemies update, queue for drawing and so overlap in place order,
/// while the blast reaches them from the last place back to the first.
/// </summary>
internal static class EnemySlotOrder
{
    /// <summary>How many enemies, counting each part of a multipart enemy, a room holds at once.</summary>
    public const int Capacity = 32;

    /// <summary>The empty place a newly spawned enemy takes: the earliest, or null when every place is taken.</summary>
    /// <param name="slots">The room's places, in order.</param>
    internal static RoomEnemySlot? FirstFree(IReadOnlyList<RoomEnemySlot> slots)
    {
        Ensure.NotNull(slots);
        foreach (RoomEnemySlot slot in slots)
        {
            if (slot.DefinitionId == EnemyDefinitionId.None)
                return slot;
        }
        return null;
    }

    /// <summary>The places in the order a Power Bomb's blast reaches them: from the last back to the first.</summary>
    /// <param name="slots">The room's places, in order.</param>
    internal static IEnumerable<RoomEnemySlot> InBlastOrder(IReadOnlyList<RoomEnemySlot> slots)
    {
        Ensure.NotNull(slots);
        for (int ordinal = slots.Count - 1; ordinal >= 0; ordinal--)
            yield return slots[ordinal];
    }
}
