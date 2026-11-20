namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room a door leads into once its own room objects are placed and before its enemies load (#2485): the owners a
/// door's room event (<see cref="DoorRoomEvent"/>) changes there. The room's load presents it.
/// </summary>
internal interface IDoorEventRoom
{
    /// <summary>Holds Samus still for the elevatube's ride.</summary>
    void HoldSamusStill();

    /// <summary>Returns the screen from the turning floor to the ordinary room display.</summary>
    void ShowOrdinaryDisplay();

    /// <summary>Starts the room's entrance treadmill block animation running <paramref name="direction"/>.</summary>
    void StartTreadmillAnimation(WreckedShipTreadmillDirection direction);

    /// <summary>Spawns the treadmill object of the entrance whose belt runs <paramref name="direction"/>.</summary>
    /// <returns>Whether a room-object place was free for it.</returns>
    bool TrySpawnEntranceTreadmill(WreckedShipTreadmillDirection direction);

    /// <summary>Spawns the elevatube's otherwise invisible room object.</summary>
    /// <returns>Whether a room-object place was free for it.</returns>
    bool TrySpawnElevatube();

    /// <summary>Restores Samus's control.</summary>
    void ReleaseSamus();
}
