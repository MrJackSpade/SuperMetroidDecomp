using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One place in the room's update order (<see cref="RoomObjectPlaces"/>) and the room object it holds: the object's
/// block and the typed object that holds its own state, what its placement gave it among it (#2091), and advances it
/// (<see cref="Behaviour"/>, #1554). A shot an object has yet to take and the terrain it saved are the object's own
/// (#2484). What the place holds is one value (#2560): nothing, a spawn's place whose setup has yet to give it its object,
/// or the object, standing in a door or not. A place that holds no object holds nothing: the object's record leaves the
/// room with it (#1622).
/// </summary>
internal sealed class RoomObjectPlace
{
    /// <summary>What the place holds; a new place is free.</summary>
    private Holding _holding = Holding.Free;

    /// <summary>
    /// The typed room object this place holds, which advances by its own steps; null for a free place, or one a spawn has
    /// taken whose setup has yet to give it its object.
    /// </summary>
    internal RoomObjectBehaviour? Behaviour => _holding switch
    {
        Holding.Held held => held.Behaviour,
        Holding.HeldInDoor held => held.Behaviour,
        _ => null,
    };

    /// <summary>
    /// The door the object at this place stands in, which Samus's arrival through it asks to stay open or close behind her
    /// (#2493): the same object as <see cref="Behaviour"/> when its owner placed it as a door (<see cref="BecomeDoor{TDoor}"/>
    /// or <see cref="Replace{TDoor}"/>), and null for an object that stands in no door or a place that holds none.
    /// </summary>
    internal IDoorBehindSamus? Door => _holding is Holding.HeldInDoor held ? held.Standing : null;

    /// <summary>
    /// Whether this place holds a room object and takes part in the room update. Only <see cref="RoomObjectPlaceList.Spawn"/>
    /// takes a place; <see cref="Delete"/> frees it.
    /// </summary>
    public bool Active => _holding is not Holding.Nothing;

    /// <summary>
    /// The room block this room object sits on and draws from: a block of the room's grid, or one past its last row in the
    /// terrain beyond the room. It is always the block's own position (its column inside the room's width); a step to a
    /// neighbouring block goes through <see cref="RoomLevelData.ObjectBlockStep"/>.
    /// </summary>
    public Position16<RoomBlocks> Block { get; set; }

    /// <summary>Makes this freshly taken place hold <paramref name="behaviour"/>, which advances it from the next update on.</summary>
    /// <exception cref="InvalidOperationException">The place is free, or already holds a typed object.</exception>
    internal void Become(RoomObjectBehaviour behaviour)
    {
        Ensure.NotNull(behaviour);
        _holding = new Holding.Held(behaviour);
    }

    /// <summary>
    /// Makes this freshly taken place hold <paramref name="door"/>, which advances it from the next update on and stands in
    /// the door at its block: Samus's arrival through that door asks it to stay open or close (#2493).
    /// </summary>
    /// <exception cref="InvalidOperationException">The place is free, or already holds a typed object.</exception>
    internal void BecomeDoor<TDoor>(TDoor door)
        where TDoor : RoomObjectBehaviour, IDoorBehindSamus
    {
        Ensure.NotNull(door);
        _holding = new Holding.HeldInDoor(door, door);
    }

    /// <summary>
    /// Makes the door at this place turn into <paramref name="next"/>, which holds the place from now on and stands in the
    /// same door: a door that turns into its blue cap. Whatever the place held before is gone.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place is free.</exception>
    internal void Replace<TDoor>(TDoor next)
        where TDoor : RoomObjectBehaviour, IDoorBehindSamus
    {
        Ensure.NotNull(next);
        if (!Active)
            throw new InvalidOperationException("Only an occupied room-object place can turn into another typed room object.");
        _holding = new Holding.HeldInDoor(next, next);
    }

    /// <summary>
    /// Takes this free place, which <see cref="RoomObjectPlaceList.Spawn"/> has chosen, for a new room object's setup: it
    /// holds no block yet, and awaits the object its setup gives it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place is already held.</exception>
    internal void Claim()
    {
        if (Active)
            throw new InvalidOperationException("A room-object place is taken only while it is free.");
        Block = default;
        _holding = Holding.AwaitingObject;
    }

    /// <summary>
    /// The object leaves the room: its place is free for the next spawn, and nothing of the object stays behind. A setup
    /// that does all its work at once leaves this way too, so the next spawn may take the same place. A place already free
    /// stays free.
    /// </summary>
    internal void Delete()
    {
        Block = default;
        _holding = Holding.Free;
    }

    /// <summary>What a room-object place holds (#2560), one of the four cases below.</summary>
    private abstract record Holding
    {
        /// <summary>Only the cases below are what a place holds.</summary>
        private Holding()
        {
        }

        /// <summary>A free place, which holds nothing.</summary>
        internal static Holding Free { get; } = new Nothing();

        /// <summary>A place a spawn has taken, whose setup has yet to give it its object or free it.</summary>
        internal static Holding AwaitingObject { get; } = new Awaiting();

        /// <summary>A free place.</summary>
        internal sealed record Nothing : Holding;

        /// <summary>A place a spawn has taken, whose setup has yet to give it its object or free it.</summary>
        internal sealed record Awaiting : Holding;

        /// <summary>A place holding <paramref name="Behaviour"/>, which stands in no door.</summary>
        /// <param name="Behaviour">The typed object the place holds.</param>
        internal sealed record Held(RoomObjectBehaviour Behaviour) : Holding;

        /// <summary>A place holding <paramref name="Behaviour"/>, which stands in the door at its block as <paramref name="Standing"/>.</summary>
        /// <param name="Behaviour">The typed object the place holds.</param>
        /// <param name="Standing">The same object, as the door Samus's arrival asks to stay open or close.</param>
        internal sealed record HeldInDoor(RoomObjectBehaviour Behaviour, IDoorBehindSamus Standing) : Holding;
    }
}
