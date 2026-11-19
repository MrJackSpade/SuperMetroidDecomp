using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A room object standing in a door, which Samus's arrival through that door asks to stay open or close behind her
/// (#1554, #2091, #2493): a coloured or grey cap, a blue cap a shot opened or another door turned into, an eye door's body,
/// or the gate Mother Brain's escape room shuts. Its owner places it as a door (<see cref="RoomObjectPlace.BecomeDoor{TDoor}"/>),
/// and the door transition's search for the first door at the door's block, in the room's update order, reads only such
/// objects (<see cref="DoorObjects.TrySpawnDoorClosingObject"/>).
/// </summary>
/// <remarks>
/// The search asks <see cref="StaysOpenBehindSamus"/> first and asks only a door that does not stay open to close: the
/// door it finds starts the closing of its own kind. Only the doors above close; an eye door's eye and bottom, the blue
/// cap and escape gate a door's closing places, and every object that is no door have no closing, so none of those stands
/// in a door.
/// </remarks>
internal interface IDoorBehindSamus
{
    /// <summary>Whether the door stays open behind Samus instead of closing, given which doors are opened for good.</summary>
    /// <param name="openedForGood">Whether a door has been opened for good.</param>
    bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood);

    /// <summary>Closes the door behind Samus: the object's closing starts on its next update.</summary>
    void CloseBehindSamus();
}
