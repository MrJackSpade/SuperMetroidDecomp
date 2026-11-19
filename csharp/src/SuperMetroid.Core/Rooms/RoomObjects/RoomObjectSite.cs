namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a placement stands its room object (#2034, #2572).</summary>
public enum RoomObjectSite
{
    /// <summary>A block of the room's grid.</summary>
    RoomGrid,

    /// <summary>
    /// A block off the room's grid, below its last row: two shipped coloured doors are placed there. Nothing in the room
    /// reaches them, so each only holds its place in the room's object order (<see cref="OffRoomDoorHeldPlaceObject"/>).
    /// </summary>
    OffTheRoom,
}
