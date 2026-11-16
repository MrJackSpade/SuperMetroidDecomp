namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Whether opening a door cap is remembered across visits to its room.</summary>
public enum DoorOpeningPersistence
{
    /// <summary>Opening the cap opens its door for good, and a door opened for good keeps the cap open when the room loads again.</summary>
    Persistent,

    /// <summary>Opening the cap records nothing: the cap is closed again whenever the room loads.</summary>
    Transient,
}
