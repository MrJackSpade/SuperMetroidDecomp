namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's room objects, each at the place it holds in the room's update order, and the spawn, the only way a place
/// is taken. Which place a spawn takes, the order the places update in and the loss of a spawn into a full room are the
/// room's <see cref="RoomObjectPlaces"/> (#1622); this list holds what each place holds.
/// </summary>
internal sealed class RoomObjectPlaceList
{
    /// <summary>The room's places, which say which hold an object and which a spawn takes.</summary>
    private readonly RoomObjectPlaces occupancy;

    /// <summary>What each place holds, in update order; each keeps its identity for the room's lifetime.</summary>
    private readonly RoomObjectPlace[] _places;

    /// <summary>Creates the room's empty room-object list over the room's places.</summary>
    /// <param name="places">The room's places, created with the room.</param>
    internal RoomObjectPlaceList(RoomObjectPlaces places)
    {
        occupancy = Ensure.NotNull(places);
        _places = [.. Enumerable.Range(0, RoomObjectPlaces.PlaceCount).Select(place => new RoomObjectPlace(places, place))];
    }

    /// <summary>
    /// Every place in update order, the place the room-object update reaches first leading, free ones included: the order
    /// the update runs in, and the order a door or a gate trigger searches for the object at a block.
    /// </summary>
    internal IReadOnlyList<RoomObjectPlace> InUpdateOrder => _places;

    /// <summary>
    /// Every place from the last the update reaches back to the first, free ones included: the order a touch or a shot
    /// searches for the collectible, station or scroll trigger at its block, and the room's other searches for a
    /// collectible or a station, run in.
    /// </summary>
    internal IEnumerable<RoomObjectPlace> FromLastPlace
    {
        get
        {
            for (int place = _places.Length - 1; place >= 0; place--)
                yield return _places[place];
        }
    }

    /// <summary>Whether every place holds an object, so every spawn is lost.</summary>
    internal bool IsFull => occupancy.IsFull;

    /// <summary>
    /// Spawns a room object: takes the first free place in update order, emptied for the object's setup, or returns null
    /// when the room is full, in which case no setup runs.
    /// </summary>
    internal RoomObjectPlace? Spawn()
    {
        if (occupancy.Take() is not { } taken)
            return null;
        RoomObjectPlace place = _places[taken];
        place.Claim();
        return place;
    }
}
