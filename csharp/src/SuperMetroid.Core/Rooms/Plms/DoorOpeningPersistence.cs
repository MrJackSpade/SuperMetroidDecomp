namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Whether opening a door cap is remembered across visits to its room.</summary>
public enum DoorOpeningPersistence
{
    /// <summary>Opening the cap sets its door bit, and a set bit keeps the cap open when the room loads again.</summary>
    Persistent,

    /// <summary>Opening the cap records nothing: the cap is closed again whenever the room loads.</summary>
    Transient,
}
