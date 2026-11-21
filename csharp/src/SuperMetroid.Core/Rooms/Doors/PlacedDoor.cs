using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The door a door object's placement names (#2091): a coloured, grey or eye door's, or the blue cap's one of those turns
/// into. When the door's opening persists the object records it opened for good, and is then spent: it keeps no door, so
/// it never finds its door opened again.
/// </summary>
/// <param name="placed">The door and its persistence, as the placement named them.</param>
internal sealed class PlacedDoor(DoorLock placed)
{
    /// <summary>The door and its persistence, as the placement named them.</summary>
    internal DoorLock Placed { get; } = Ensure.NotNull(placed);

    /// <summary>Whether the object recorded its door opened for good, after which it keeps no door.</summary>
    internal bool Spent { get; set; }


    /// <summary>Whether the door has been opened for good: never for one whose opening does not persist, or once spent.</summary>
    /// <param name="openedForGood">Whether a door has been opened for good.</param>
    internal bool IsOpenedForGood(Func<LockedDoor, bool> openedForGood) =>
        !Spent && Placed.Persists && Ensure.NotNull(openedForGood)(Placed.Door);

    /// <summary>Records the door opened for good when its opening persists, which spends the object.</summary>
    internal void RecordOpened(SharedGameState system)
    {
        Ensure.NotNull(system);
        if (Spent || !Placed.Persists)
            return;
        system.OpenDoor(Placed.Door);
        Spent = true;
    }
}
