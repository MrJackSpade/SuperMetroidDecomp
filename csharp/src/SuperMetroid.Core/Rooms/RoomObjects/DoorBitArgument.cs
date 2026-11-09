using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The door a cap stands for, and whether opening the cap opens the door for good.</summary>
/// <param name="Door">The door the cap stands for.</param>
/// <param name="Persistence">Whether opening the cap records the door as opened and an opened door keeps the cap open.</param>
public sealed record DoorBitArgument(LockedDoor Door, DoorOpeningPersistence Persistence) : RoomObjectRoomArgument
{
    /// <summary>Whether opening the cap records the door as opened and an opened door keeps the cap open.</summary>
    public bool Persists => Persistence == DoorOpeningPersistence.Persistent;
}
