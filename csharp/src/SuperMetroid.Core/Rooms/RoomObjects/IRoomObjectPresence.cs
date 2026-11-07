namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Queries the active room PLM population.</summary>
public interface IRoomObjectPresence
{
    /// <summary>Whether a PLM with <paramref name="header"/> is active.</summary>
    bool HasActiveHeader(RoomObjectHeaderId header);
}
