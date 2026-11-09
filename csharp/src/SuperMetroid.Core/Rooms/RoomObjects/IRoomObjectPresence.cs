namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Queries the active room-object population.</summary>
public interface IRoomObjectPresence
{
    /// <summary>Whether a room object with <paramref name="header"/> is active.</summary>
    bool HasActiveHeader(RoomObjectHeaderId header);
}
