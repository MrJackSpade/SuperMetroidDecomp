using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One place in the room's update order (<see cref="RoomObjectPlaces"/>) and the room object it holds: the object's
/// block, what it was placed with, a shot on its block it has yet to take, and the typed object that holds its own state
/// and advances it (<see cref="Behaviour"/>, #1554). A place that holds no object holds nothing: the object's record
/// leaves the room with it (#1622).
/// </summary>
internal sealed class RoomObjectSlot
{
    /// <summary>The room's places, which say whether this one holds an object.</summary>
    private readonly RoomObjectPlaces _places;

    /// <summary>This place, numbered in update order.</summary>
    private readonly int _place;

    /// <summary>Creates the free place <paramref name="place"/> of the room's <paramref name="places"/>.</summary>
    internal RoomObjectSlot(RoomObjectPlaces places, int place)
    {
        _places = Ensure.NotNull(places);
        _place = place;
    }

    /// <summary>
    /// The typed room object this place holds, which advances by its own steps; null for a free place, or one whose setup
    /// did all its work.
    /// </summary>
    internal RoomObjectBehaviour? Behaviour { get; private set; }
    /// <summary>
    /// Whether this place holds a room object and takes part in the room update. Only <see cref="RoomObjectSlotTable.Spawn"/>
    /// takes a place; <see cref="Delete"/> frees it.
    /// </summary>
    public bool Active => _places.IsHeld(_place);
    /// <summary>
    /// The identity the object's spawner gave it, <see cref="RoomObjectHeaderId.None"/> for one spawned by what it does.
    /// No room object, family or search reads it (#1622): the room's population selects a setup by its record's kind
    /// and hands that kind to the setup.
    /// </summary>
    public RoomObjectHeaderId Header { get; set; }
    /// <summary>
    /// The room block this room object sits on and draws from: a block of the room's grid, or one past its last row in the
    /// terrain beyond the room. It is always the block's own position (its column inside the room's width); a step to a
    /// neighbouring block goes through <see cref="RoomLevelData.ObjectBlockStep"/>.
    /// </summary>
    public Position16<RoomBlocks> Block { get; set; }
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

    /// <summary>Makes this freshly taken place hold <paramref name="behaviour"/>, which advances it from the next update on.</summary>
    /// <exception cref="InvalidOperationException">The place is free, or already holds a typed object.</exception>
    internal void Become(RoomObjectBehaviour behaviour)
    {
        Ensure.NotNull(behaviour);
        if (!Active || Behaviour is not null)
            throw new InvalidOperationException("Only a freshly taken room-object place can become a typed room object.");
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
    /// Readies this place, which <see cref="RoomObjectSlotTable.Spawn"/> has just taken, for a new room object's setup.
    /// Only the spawn calls it, which analyzer SME6303 enforces.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place was not taken.</exception>
    internal void Claim()
    {
        if (!Active)
            throw new InvalidOperationException("A room-object place is readied only once a spawn has taken it.");
        ClearRecord();
    }

    /// <summary>
    /// The object leaves the room: its place is free for the next spawn, and nothing of the object stays behind. A setup
    /// that does all its work at once leaves this way too, so the next spawn may take the same place.
    /// </summary>
    internal void Delete()
    {
        if (Active)
            _places.Free(_place);
        ClearRecord();
    }

    /// <summary>Clears what the place holds.</summary>
    private void ClearRecord()
    {
        Header = RoomObjectHeaderId.None;
        Block = default;
        RestoreTerrain = null;
        Argument = null;
        ProjectileHit = null;
        Behaviour = null;
    }
}
