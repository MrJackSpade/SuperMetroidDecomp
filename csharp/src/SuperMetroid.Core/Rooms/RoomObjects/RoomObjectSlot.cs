using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One place in the room's update order (<see cref="RoomObjectSlotTable"/>) and the room object it holds: the object's
/// identity and block, what it was placed with, a shot on its block it has yet to take, and the typed object that holds
/// its own state and advances it (<see cref="Behaviour"/>, #1554).
/// </summary>
internal sealed class RoomObjectSlot
{
    /// <summary>
    /// The typed room object this place holds, which advances by its own steps; null for a free place, or one whose setup
    /// did all its work.
    /// </summary>
    internal RoomObjectBehaviour? Behaviour { get; private set; }
    /// <summary>
    /// Whether this slot holds a room object and participates in the room update. Only <see cref="RoomObjectSlotTable"/>'s
    /// spawn operations claim a slot; <see cref="Delete"/> and <see cref="Clear"/> free it.
    /// </summary>
    public bool Active { get; private set; }
    /// <summary>The room object the slot holds, selecting its setup and behavior.</summary>
    public RoomObjectHeaderId Header { get; set; }
    /// <summary>Index of the room block owned by this room object.</summary>
    public int BlockIndex { get; set; }
    /// <summary>
    /// The terrain the slot restores at its block, or null when its object saved none. Grapple setup saves the original
    /// block; collision-bomb setup saves the dimension parent instead. Only an object whose setup saved a block restores
    /// one (<see cref="RoomObjectStepContext.DrawRestoredBlock"/>).
    /// </summary>
    public RoomTerrainBlock? RestoreTerrain { get; set; }
    /// <summary>What the slot's room object was placed with, or the state its setup or opening left there; null for none.</summary>
    public RoomObjectRoomArgument? Argument { get; set; }
    /// <summary>The projectile hit on the place's block that its object has not yet taken, or null for none.</summary>
    public ResidentObjectProjectileHit? ProjectileHit { get; set; }

    /// <summary>
    /// Clears every field, so a recycled place cannot run its former object. Every population and runtime spawn passes
    /// through this reset before setting its own state.
    /// </summary>
    internal void Clear()
    {
        Active = false;
        Header = 0;
        BlockIndex = 0;
        RestoreTerrain = null;
        Argument = null;
        ProjectileHit = null;
        Behaviour = null;
    }

    /// <summary>Makes this freshly claimed place hold <paramref name="behaviour"/>, which advances it from the next update on.</summary>
    /// <exception cref="InvalidOperationException">The place is free, or already holds a typed object.</exception>
    internal void Become(RoomObjectBehaviour behaviour)
    {
        Ensure.NotNull(behaviour);
        if (!Active || Behaviour is not null)
            throw new InvalidOperationException("Only a freshly claimed room-object place can become a typed room object.");
        Behaviour = behaviour;
    }

    /// <summary>
    /// Makes the object at this place turn into <paramref name="next"/>, which holds the place from now on: a door that
    /// turns into its blue cap. Whatever the place held before is gone.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place is free.</exception>
    internal void Replace(RoomObjectBehaviour next)
    {
        Ensure.NotNull(next);
        if (!Active)
            throw new InvalidOperationException("Only an occupied room-object place can turn into another typed room object.");
        Behaviour = next;
    }

    /// <summary>
    /// Claims this free slot for a new room object: every field is reset and the slot is active. Only <see cref="RoomObjectSlotTable"/>'s
    /// spawn operations call it, which analyzer SME6303 enforces.
    /// </summary>
    internal void Claim()
    {
        if (Active)
            throw new InvalidOperationException("An occupied room-object slot cannot be claimed.");
        Clear();
        Active = true;
    }

    /// <summary>
    /// Deletes the room object, as the delete instruction does: only occupancy is cleared, and the other fields stay until the
    /// slot is claimed again.
    /// </summary>
    internal void Delete() => Active = false;
}
