namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's object update order: the places its room objects hold, and the three spawn operations, the only way a
/// place is taken.
/// </summary>
/// <remarks>
/// <para>
/// The update-order rule (#1554). The room holds at most <see cref="Capacity"/> objects, each at a fixed place. Every
/// pass over them runs in one order, the update order: the room-object update, every search for the object that owns
/// a block, and every spawn's search for a free place. A spawn takes the first free place in that order, so in a room
/// where nothing has been removed the objects update in the order they were spawned, and a spawn after a removal takes
/// the removed object's place.
/// </para>
/// <para>
/// The rule is kept because the game observes it. A room population's records spawn in authored order and update in
/// it, and a record whose setup removes its object at once (every scroll extension) leaves its place to the next
/// record. A spawn made during the room-object update into a place the update has still to reach runs on that same
/// update, and into one it has passed, on the next. Two requests made in one update, such as the pair of mural rows
/// Mother Brain's ascent asks for, update in the order of the places they took. A search for the object at a block
/// (a gate's trigger finding its gate, a touch finding its station) takes the first match in the order. Nothing else
/// about the places is part of the game.
/// </para>
/// <para>
/// The full-room rule (#1608). A full room drops a spawn: a gate trigger's shot, a Speed Booster block's contact and a
/// boss's terrain change are lost then, as the original game loses them. The rule is kept because a boss room can be
/// full when a boss spawns its change. The room's own population and the boss's concurrently live changes stay far
/// below <see cref="Capacity"/>, but the room's occupancy is not bounded by them: every block reaction Samus starts
/// (a bombed, shot, crumbled or Speed Booster block, a sand contact) holds a place until its own updates end it, and
/// while room-object updates are suspended (X-Ray, and G-Mode, which keeps them suspended across a door into the
/// next room while enemies, bosses among them, keep running) none of those places is ever released. Repeated
/// reactions then fill the room, and a boss that mutates its room meanwhile finds no free place.
/// </para>
/// </remarks>
internal sealed class RoomObjectSlotTable
{
    /// <summary>How many room objects the room can hold at once; a spawn into a full table fails.</summary>
    internal const int Capacity = 40;

    /// <summary>The slots by ordinal; each slot object keeps its identity for the table's lifetime.</summary>
    private readonly RoomObjectSlot[] _slots = Enumerable
        .Range(0, Capacity)
        .Select(_ => new RoomObjectSlot())
        .ToArray();

    /// <summary>The slots by ordinal, 0 through <see cref="Capacity"/> - 1.</summary>
    internal IReadOnlyList<RoomObjectSlot> Slots => _slots;

    /// <summary>The slots in processing order, highest ordinal first, including free ones.</summary>
    internal IEnumerable<RoomObjectSlot> InProcessingOrder
    {
        get
        {
            for (int ordinal = _slots.Length - 1; ordinal >= 0; ordinal--)
                yield return _slots[ordinal];
        }
    }

    /// <summary>Whether every slot is occupied, so every spawn fails.</summary>
    internal bool IsFull => ActiveCount == Capacity;

    /// <summary>
    /// A room object spawned by a fixed game event: claims the first free slot in processing order, or returns null when the
    /// table is full, in which case no setup runs.
    /// </summary>
    internal RoomObjectSlot? SpawnHardcoded() => ClaimFirstFree();

    /// <summary>
    /// A room object placed by room population or by the door-closing room object: claims the first free slot in processing order, or
    /// returns null when the table is full, in which case no setup runs.
    /// </summary>
    internal RoomObjectSlot? SpawnRoomObject() => ClaimFirstFree();

    /// <summary>
    /// A room object spawned by a block reaction at the reacting block: claims the first free slot in processing order, or
    /// returns null when the table is full, in which case no setup runs.
    /// </summary>
    internal RoomObjectSlot? SpawnAtCurrentBlock() => ClaimFirstFree();

    /// <summary>The shared search of all three spawns: the first free slot in processing order is claimed.</summary>
    private RoomObjectSlot? ClaimFirstFree()
    {
        foreach (RoomObjectSlot slot in InProcessingOrder)
        {
            if (slot.Active)
                continue;
            slot.Claim();
            return slot;
        }
        return null;
    }

    /// <summary>Number of occupied slots.</summary>
    public int ActiveCount
    {
        get
        {
            int count = 0;
            foreach (RoomObjectSlot slot in _slots)
                if (slot.Active) count++;
            return count;
        }
    }
}
