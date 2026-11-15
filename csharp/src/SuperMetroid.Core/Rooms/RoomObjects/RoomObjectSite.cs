namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a placement stands its room object (#2034).</summary>
public enum RoomObjectSite
{
    /// <summary>A block of the room's grid.</summary>
    RoomGrid,

    /// <summary>
    /// A block of the terrain beyond the room, below its last row: two shipped coloured doors are placed there, where their
    /// setup and draws write terrain nothing in the room reaches.
    /// </summary>
    BeyondTheRoom,
}
